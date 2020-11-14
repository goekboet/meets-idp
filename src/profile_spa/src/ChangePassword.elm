module ChangePassword exposing (Model, Msg, init, update, view)

import Http exposing (Error, Header)
import Html exposing (Html)
import Html.Attributes as Attr exposing (class, disabled)
import Components exposing (StepStatus(..), ChangeStatus(..), completionStep, completionProgress, input, Input)
import Html.Events exposing (onClick, onInput)
import Model exposing (Profile)
import Url.Builder as UrlB
import Json.Encode as Encode exposing (Value)

type alias Password = String

isDirty : Password -> Bool
isDirty p = p /= "" 

type alias ChangePasswordInput =
    { currentPassword : Password
    , newPassword : Password
    , newPasswordRepeat : Password
    }

toJson : ChangePasswordInput -> Value
toJson inp =
    Encode.object
        [ ("old", Encode.string inp.currentPassword)
        , ("new", Encode.string inp.newPassword)
        ]

emptyInput : ChangePasswordInput
emptyInput =
    { currentPassword = ""
    , newPassword = ""
    , newPasswordRepeat = ""
    }

type ApiStatus
    = CollectingInput
    | Pending
    | Errored
    | Submitted

apistatusRejection : ApiStatus -> Rejection
apistatusRejection s = 
    case s of
    Errored -> "This is not the current password"
    _ -> ""

type alias Model = (ApiStatus, ChangePasswordInput)

init : Model
init = (CollectingInput, emptyInput)

updateCurrentPassword : Model -> Password -> Model 
updateCurrentPassword (_, inp) pwd = (CollectingInput, {inp | currentPassword = pwd})

updateNewPassword : Model -> Password -> Model 
updateNewPassword (_, inp) pwd = (CollectingInput, {inp | newPassword = pwd})

updateRepeatedPassword : Model -> Password -> Model 
updateRepeatedPassword (_, inp) pwd = (CollectingInput, {inp | newPasswordRepeat = pwd})


type Msg 
    = Noop
    | CurrentPasswordInputChanged Password
    | NewPasswordInputChanged Password
    | NewPasswordRepeatChanged String
    | SubmitPasswordChange
    | PasswordChanged (Result Error ())
    | PasswordChangeCanceled

completionStatus : (Msg -> msg) -> Profile -> Model -> Html msg
completionStatus toApp p m =
    completionProgress
        (completionStep Complete NotApplicable (toApp Noop) "Email" (Just (p.email ++ " (verified)")))
        (completionStep Complete Changing (toApp Noop) "Password" (Just "added"))
        (completionStep Complete Applicable (toApp Noop) "Name" (Just p.name))
        (completionStep Complete Applicable (toApp Noop) "Profile complete" Nothing)
        Nothing

type alias Rejection = String

passwordRejection : Password -> Rejection
passwordRejection newPassword =
    if (String.length newPassword < 8) then "Password must be at least 8 characters long."
    else if (String.length newPassword > 64) then "Password must not be longer than 64 characters."
    else ""

repeatPasswordRejection : Password -> Password -> Rejection
repeatPasswordRejection newPassword passwordRepeat = 
    if newPassword /= passwordRepeat 
    then "Passwords don't match" 
    else ""

currentPasswordInput : Input
currentPasswordInput =
    { id = "CurrentPwd"
    , type_ = "password"
    , icon = "lock_open"
    , label = "Current password"
    , tabindex = 1
    , autofocus = True
    }

passwordInput : Input
passwordInput =
    { id = "NewPwd"
    , type_ = "password"
    , icon = "lock"
    , label = "Password"
    , tabindex = 2
    , autofocus = False
    }

repeatPasswordInput : Input
repeatPasswordInput =
    { id = "RepeatPwd"
    , type_ = "password"
    , icon = "lock_outline"
    , label = "Repeat password"
    , tabindex = 3
    , autofocus = False
    }

antiCsrfHeader : String ->  Header
antiCsrfHeader t =
    Http.header "RequestVerificationToken" t

changePasswordApiCall : AntiCsrfToken -> (Msg -> msg) -> ChangePasswordInput -> Cmd msg
changePasswordApiCall t toApp inp =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "changePassword" ] []
    , body = toJson inp |> Http.jsonBody
    , expect = Http.expectWhatever (toApp << PasswordChanged)
    , timeout = Nothing
    , tracker = Nothing
    }

changePasswordForm : (Msg -> msg) -> Model -> Html msg
changePasswordForm toApp (s, inp) =
    let
        currentPasswordReject = apistatusRejection s
        pwdReject = passwordRejection inp.newPassword
        pwdRepReject = repeatPasswordRejection inp.newPassword inp.newPasswordRepeat
        rejected = 
            [ pwdReject, pwdRepReject ] 
            |> String.concat 
            |> String.isEmpty
            |> not

        pending = 
            case s of
            Pending -> True
            _ -> False
    in
    
    Html.div [ class "section" ] 
    [ input 
        currentPasswordInput 
        (isDirty inp.currentPassword) 
        (if currentPasswordReject /= ""
         then currentPasswordReject
         else passwordRejection inp.currentPassword) 
        (toApp << CurrentPasswordInputChanged)
    , input 
        passwordInput 
        (isDirty inp.newPassword) 
        (passwordRejection inp.newPassword) 
        (toApp << NewPasswordInputChanged) 
    , input 
        repeatPasswordInput 
        (isDirty inp.newPasswordRepeat) 
        (repeatPasswordRejection inp.newPassword inp.newPasswordRepeat) 
        (toApp << NewPasswordRepeatChanged)
    , Html.button 
      [ class "btn"
      , Attr.name "SubmitPwd"
      , Attr.style "margin-right" "0.5em"
      , onClick (toApp SubmitPasswordChange)
      , Attr.disabled (rejected || pending)
      ] 
      [ Html.text "Submit" ]
    , Html.button 
      [ class "btn-flat"
      , Attr.name "Cancel"
      , onClick (toApp PasswordChangeCanceled)
      , Attr.disabled pending
      ] 
      [ Html.text "Cancel" ]
    ]

view : (Msg -> msg) -> Profile -> Model -> List (Html msg)
view toApp p m =
    [ completionStatus toApp p m 
    , Html.h5 [] [ Html.text "Change password"]
    , Html.p [] [ Html.text "If you can, let a password manager generate a long and random one for you." ]
    , changePasswordForm toApp m
    ]

type alias AntiCsrfToken = String

update : AntiCsrfToken -> (Msg -> msg) -> Msg -> Model -> (Model, Cmd msg, Bool)
update t toApp msg model =
    case msg of
    Noop -> (model, Cmd.none, False)
    CurrentPasswordInputChanged pwd -> ((updateCurrentPassword model pwd), Cmd.none, False)
    NewPasswordInputChanged pwd -> ((updateNewPassword model pwd), Cmd.none, False)
    NewPasswordRepeatChanged pwd -> ((updateRepeatedPassword model pwd), Cmd.none, False)
    SubmitPasswordChange -> (model, changePasswordApiCall t toApp (Tuple.second model), False)
    PasswordChanged (Ok _) -> (model, Cmd.none, True)
    PasswordChanged (Err _) -> ((Errored, Tuple.second model), Cmd.none, False)
    PasswordChangeCanceled -> (model, Cmd.none, True)