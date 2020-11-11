module NewPassword exposing (Model, Msg, init, update, view)

import Http exposing (Error, Header)
import Components exposing (StepStatus(..), ChangeStatus(..), completionStep, completionProgress, input, Input)
import Model exposing (Profile)
import Html exposing (Html)
import Html.Attributes as Attr exposing (class, disabled)
import Html.Events exposing (onClick, onInput)
import Url.Builder as UrlB
import Json.Encode as Encode exposing (Value)

type alias Password = String
type alias Rejection = String 

type alias NewPasswordInput =
    { newPassword : Password
    , passwordRepeat : Password
    }

isDirty : NewPasswordInput -> Bool
isDirty { newPassword, passwordRepeat } 
    = 
    let 
        gt0 x = x > 0
    in 
    String.concat [ newPassword, passwordRepeat ] |> String.length |> gt0

passwordRejection : NewPasswordInput -> Rejection
passwordRejection { newPassword, passwordRepeat } =
    if (String.length newPassword < 8) then "Password must be at least 8 characters long."
    else if (String.length newPassword > 64) then "Password must not be longer than 64 characters."
    else ""

repeatPasswordRejection : NewPasswordInput -> Rejection
repeatPasswordRejection { newPassword, passwordRepeat } = 
    if newPassword /= passwordRepeat 
    then "Passwords don't match" 
    else ""

type ApiStatus 
    = CollectingInput
    | Pending
    | Error

type alias Model = (ApiStatus, NewPasswordInput)

getInput : Model -> NewPasswordInput
getInput = Tuple.second

init : Model
init = 
    ( CollectingInput
    , { newPassword = "", passwordRepeat = "" } 
    )

type Msg
    = NewPasswordChange String
    | PasswordRepeatChange String
    | SubmitAddPassword
    | AddPasswordResponse (Result Error ())
    | Noop



update : AntiCsrfToken -> (Msg -> msg) -> Msg -> Model -> (Model, Cmd msg, Bool)
update anticsrf toApp msg (api, inp) =
    case msg of
    NewPasswordChange s -> 
        (setNewPassword s (api, inp), Cmd.none, False)
    PasswordRepeatChange s -> 
        (setPwdRepeat s (api, inp), Cmd.none, False)
    SubmitAddPassword -> 
        ((Pending, inp), addPasswordApiCall anticsrf toApp inp, False )
    AddPasswordResponse (Ok _) -> 
        ((api, inp), Cmd.none, True)
    AddPasswordResponse (Err _) -> 
        ((Error, inp), Cmd.none, False)
    Noop -> ((api, inp), Cmd.none, False)

type alias AntiCsrfToken = String

antiCsrfHeader : String ->  Header
antiCsrfHeader t =
    Http.header "RequestVerificationToken" t

passwordJson : Password -> Value
passwordJson pwd =
    Encode.object [ ("password", Encode.string pwd) ]

addPasswordApiCall : AntiCsrfToken -> (Msg -> msg) -> NewPasswordInput -> Cmd msg
addPasswordApiCall t toApp { newPassword, passwordRepeat } =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "password" ] []
    , body = passwordJson newPassword |> Http.jsonBody
    , expect = Http.expectWhatever (toApp << AddPasswordResponse)
    , timeout = Nothing
    , tracker = Nothing
    }

setNewPassword : Password -> Model -> Model
setNewPassword pwd (s, inp) = (s, { inp | newPassword = pwd })

setPwdRepeat : Password -> Model -> Model
setPwdRepeat pwd (s, inp) = (s, { inp | passwordRepeat = pwd })

completionStatus : (Msg -> msg) -> String -> Model -> Html msg
completionStatus toApp email m =
    completionProgress
        (completionStep Complete NotApplicable (toApp Noop) "Email" (Just (email ++ " (verified)")))
        (completionStep Current NotApplicable (toApp Noop) "Password" (Just "not added"))
        (completionStep InComplete NotApplicable (toApp Noop) "Name" (Just "not added"))
        (completionStep InComplete NotApplicable (toApp Noop) "Profile complete" Nothing)
        Nothing

passwordInput : Input
passwordInput =
    { id = "NewPwd"
    , type_ = "password"
    , icon = "lock"
    , label = "Password"
    , tabindex = 1
    , autofocus = True
    }

repeatPasswordInput : Input
repeatPasswordInput =
    { id = "RepeatPwd"
    , type_ = "password"
    , icon = "lock_outline"
    , label = "Repeat password"
    , tabindex = 2
    , autofocus = False
    }
    

addPasswordForm : (Msg -> msg) -> Model -> Html msg
addPasswordForm toApp (s, inp) =
    let
        pwdReject = passwordRejection inp
        pwdRepReject = repeatPasswordRejection inp
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
        passwordInput 
        (isDirty inp) 
        (passwordRejection inp) 
        (toApp << NewPasswordChange) 
    , input 
        repeatPasswordInput 
        (isDirty inp) 
        (repeatPasswordRejection inp) 
        (toApp << PasswordRepeatChange)
    , Html.button 
      [ class "btn"
      , Attr.name "SubmitPwd"
      , onClick (toApp SubmitAddPassword)
      , Attr.disabled (rejected || pending)
      ] 
      [ Html.text "Add" ]
    ]

view : (Msg -> msg) -> String -> Model -> List (Html msg)
view toApp email m =
    [ completionStatus toApp email m 
    , Html.h5 [] [ Html.text "Add password"]
    , Html.p [] [ Html.text "If you can, let a password manager generate a long and random one for you." ]
    , addPasswordForm toApp m
    ]


