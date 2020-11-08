module Main exposing (main)

import Browser exposing (Document)
import Html exposing (Html)
import Html.Attributes as Attr exposing (class, disabled)
import Html.Events exposing (onClick, onInput )
import Http exposing (Error, Header)
import Json.Decode as Decode exposing (Decoder)
import Json.Encode as Encode exposing (Value)
import Url.Builder as UrlB
import Html exposing (Attribute)
import Html.Attributes exposing (name)
import Html exposing (p)

type ApiResponse a =
    Pending
    | Errored
    | Received a

type alias OidcLogin =
    { id : String
    , name : String
    }

resumeLogin : Profile -> OidcLogin -> Html Msg
resumeLogin p login =
    let
      profileIncomplete = (not p.hasPassword) || (p.name == "" )  
    in
    if profileIncomplete
    then 
        Html.a [] 
        [ Html.text (String.concat ["Log in to ", login.name])]
    else
        Html.a 
        [ Attr.href login.id 
        ] 
        [ Html.text (String.concat ["Log in to ", login.name])]

type alias Flags =
    { antiCsrf : String 
    , oidcLogin : Maybe OidcLogin
    }

type alias Profile = 
    { name : String
    , email : String
    , hasPassword : Bool
    }

decodeProfile : Decoder Profile
decodeProfile =
    Decode.map3 Profile
        (Decode.field "name" Decode.string)
        (Decode.field "email" Decode.string)
        (Decode.field "hasPassword" Decode.bool)
    

emptyProfile : Profile
emptyProfile =
    { name = ""
    , email = ""
    , hasPassword = False
    }

passwordJson : String -> Value
passwordJson pwd =
    Encode.object [ ("password", Encode.string pwd) ]

changePasswordJson : CurrentPassword -> NewPassword -> Value
changePasswordJson current new =
    Encode.object
        [ ("old", Encode.string current )
        , ("new", Encode.string new )
        ]

nameJson : String -> Value
nameJson n =
    Encode.object [ ("name", Encode.string n) ]

passwordLabel : Bool -> Html Msg
passwordLabel has =
    let
        password s = Html.p [] [ Html.b [] [ Html.text "password: "], Html.text s ]
    in
        if has 
        then password "added" 
        else password "not added" 


newPwdValidation : String -> String
newPwdValidation pwd =
    if (String.length pwd < 8) then "Password must be at least 8 characters long."
    else if (String.length pwd > 64) then "Password must not be longer than 64 characters."
    else ""

newPwdRepeatValidation : String -> String -> String
newPwdRepeatValidation np pr = if np == pr then "" else "Passwords don't match"

newPwdInput : String -> Input 
newPwdInput np = { value = np, validationMsg = newPwdValidation np, dirty = np /= "" }

repeatPwdInput : String -> String -> Input 
repeatPwdInput np pr = { value = pr, validationMsg = newPwdRepeatValidation np pr, dirty = pr /= "" }

passwordControl : PasswordState -> Html Msg
passwordControl state =
    case state of
    PasswordUnavailiable -> Html.text ""
    AddPasswordInput np pr active ->
        if active
        then
            let
                pwdInput = newPwdInput np
                pwdRepeatInput = repeatPwdInput np pr
            in  
                Html.div [ class "section" ]
                [ input "lock" "NewPwd" "New password" "password" pwdInput AddPwdNew
                , input "lock_outline" "NewPwdRepeat" "Repeat new password" "password" pwdRepeatInput AddPwdRepeat
                , Html.button 
                  [ class "btn"
                  , onClick AddPwdSubmit
                  , disabled (unSubmittable [ pwdInput, pwdRepeatInput ]) ] 
                  [ Html.text "Add" ]
                , Html.button 
                  [ class "btn-flat"
                  , onClick (ChangePasswordState (AddPasswordInput np pr False))
                  ] 
                  [ Html.text "Cancel" ]
                ]
        else
            Html.button 
            [ class "btn"
            , onClick (ChangePasswordState (AddPasswordInput np pr True))
            ] 
            [ Html.text "Add"]
    ChangePasswordInput cp np pr active ->
        if active
        then
            let
                currentPwdInput = newPwdInput cp
                pwdInput = newPwdInput np
                pwdRepeatInput = repeatPwdInput np pr
            in  
                Html.div [ class "section" ]
                [ input "lock_open" "CurrentPwd" "Current password" "password" currentPwdInput ChangePwdCurrent
                , input "lock" "NewPwd" "New password" "password" pwdInput ChangePwdNew
                , input "lock_outline" "NewPwdRepeat" "Repeat new password" "password" pwdRepeatInput ChangePwdRepeat
                , Html.button 
                  [ class "btn"
                  , onClick ChangePwdSubmit
                  , disabled (unSubmittable [ currentPwdInput, pwdInput, pwdRepeatInput ]) 
                  ] 
                  [ Html.text "Change" ]
                , Html.button 
                  [ class "btn-flat"
                  , onClick (ChangePasswordState (ChangePasswordInput cp np pr False))
                  ] [ Html.text "Cancel" ]         
                ]
        else
            Html.button 
            [ class "btn" 
            , onClick (ChangePasswordState (ChangePasswordInput cp np pr True))] 
            [ Html.text "Change"]
    PasswordPending ->
        spinner
    PasswordError ->
        Html.button [ disabled True] [ Html.text "Error" ]

antiCsrfHeader : String ->  Header
antiCsrfHeader t =
    Http.header "RequestVerificationToken" t

submitAddPassword : AntiCsrfToken -> PasswordState -> Cmd Msg
submitAddPassword anticsrf state =
    case state of
    AddPasswordInput np _ _ -> addPasswordApiCall anticsrf np
    _ -> Cmd.none

addPasswordApiCall : AntiCsrfToken -> NewPassword -> Cmd Msg
addPasswordApiCall t pwd =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "password" ] []
    , body = passwordJson pwd |> Http.jsonBody
    , expect = Http.expectWhatever AddPwdResponse
    , timeout = Nothing
    , tracker = Nothing
    }

submitChangePassword : AntiCsrfToken -> PasswordState -> Cmd Msg
submitChangePassword anticsrf state =
    case state of
    ChangePasswordInput cp np _ _ -> changePasswordApiCall anticsrf cp np
    _ -> Cmd.none

changePasswordApiCall : AntiCsrfToken -> CurrentPassword -> NewPassword -> Cmd Msg
changePasswordApiCall t cp np =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "changePassword" ] []
    , body = changePasswordJson cp np |> Http.jsonBody
    , expect = Http.expectWhatever ChangePwdResponse
    , timeout = Nothing
    , tracker = Nothing
    }

submitChangeName : AntiCsrfToken -> NameState -> Cmd Msg
submitChangeName anticsrf state =
    case state of
    NameInput _ nn _ -> changeNameApiCall anticsrf nn
    _ -> Cmd.none

changeNameApiCall : AntiCsrfToken -> NewName -> Cmd Msg
changeNameApiCall t nn =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "name" ] []
    , body = nameJson nn |> Http.jsonBody
    , expect = Http.expectWhatever ChangeNameResponse
    , timeout = Nothing
    , tracker = Nothing
    }

nameInput : OriginalName -> NewName -> Input
nameInput on nn =
    let
        validate n =
            if (String.length n == 0) then "Name must not be empty."
            else if (String.length n > 256) then "Password must not be longer than 256 characters."
            else "" 

    in
        { value = nn, validationMsg = validate nn, dirty = on /= nn }

nameLabel : OriginalName -> Html Msg
nameLabel on =
    let
        name s = Html.p [] [ Html.b [] [ Html.text "name: "], Html.text s ]
    in
        if on /= "" 
        then name on 
        else name "not added" 

nameButtonLabel : OriginalName -> String
nameButtonLabel on =
    if on == ""
    then "Add"
    else "Change"

nameControl : NameState -> Html Msg
nameControl state =
    case state of
    NameUnavailiable -> Html.text ""
    NameInput on nn a ->
        if a
        then
            let
                newNameInput = nameInput on nn
            in        
                Html.div [ class "section" ]
                [ input "person_outline" "NewName" "Name" "text" newNameInput ChangeNameNew
                , Html.button 
                  [ class "btn"
                  , onClick ChangeNameSubmit
                  , disabled (unSubmittable [ newNameInput ]) 
                  ] 
                  [ Html.text (nameButtonLabel on) ]
                , Html.button 
                  [ class "btn-flat"
                  , onClick (ChangeNameState (NameInput on nn False))
                  ] 
                  [ Html.text "Cancel" ]
                ]
        else
            Html.button 
              [ class "btn"
              , onClick (ChangeNameState (NameInput on nn True))
              ] 
              [ Html.text (nameButtonLabel on) ]
    NamePending -> 
        spinner
    NameError -> 
        Html.button [ class "btn",  disabled True ] [ Html.text "Error" ]

internalLink : Maybe OidcLogin -> String -> String -> Html Msg
internalLink login url name =
    let
        returnUrl = Maybe.map .id login |> Maybe.withDefault ""
    in
        Html.a [ Attr.href url ] [ Html.text name ]

renderProfile : Maybe OidcLogin -> Profile -> PasswordState -> NameState -> (List (Html Msg))
renderProfile oidcLogin p pwdState nameState =
    [ Html.h5 [] [ Html.text p.email ]
    , passwordLabel p.hasPassword
    , passwordControl pwdState
    , nameLabel p.name
    , nameControl nameState
    , Html.ul [] 
      [ Html.li [] [ internalLink oidcLogin "/ChangeUsername" "Change email" ]
      , Html.li [] [ internalLink oidcLogin "Unregister" "Delete your account" ]
      ]
    , Maybe.map (resumeLogin p) oidcLogin |> Maybe.withDefault (Html.text "")
    ]

type alias Input =
    { value : String
    , validationMsg : String
    , dirty : Bool
    }

unSubmittable : List Input -> Bool
unSubmittable inputs =
    List.any (\x -> (x.dirty |> not) || (String.isEmpty x.validationMsg |> not)) inputs

type alias InputType = String
type alias InputName = String
type alias InputDisplay = String

inputLabel : String -> Bool -> String
inputLabel title dirty =
    if dirty
    then String.concat [ title, " ", "*" ]
    else title


input : String -> InputName -> InputDisplay -> InputType -> Input -> (String -> Msg) -> Html Msg
input icon_name n d t inp change =
    Html.div [ class "input-field" ] 
    [ Html.i [ class "material-icons prefix" ] [ Html.text icon_name ]
    , Html.input [ class "validate", Attr.placeholder d, Attr.name n, Attr.type_ t, onInput change ] []
    , Html.label [ class "active", Attr.for n ] [ Html.text (inputLabel d inp.dirty)]
    , Html.span [ class "helper-text" ] [ Html.text inp.validationMsg ]
    ]

emptyInput : Input
emptyInput = { value = "", validationMsg = "", dirty = False }

type alias CurrentPassword = String
type alias NewPassword = String
type alias NewPasswordRepeat = String

type PasswordState
    = PasswordUnavailiable
    | AddPasswordInput NewPassword NewPasswordRepeat Bool
    | ChangePasswordInput CurrentPassword NewPassword NewPasswordRepeat Bool
    | PasswordPending
    | PasswordError

initPasswordState : PasswordState
initPasswordState = PasswordUnavailiable

profilePasswordState : Profile -> PasswordState
profilePasswordState p =
    if p.hasPassword
    then ChangePasswordInput "" "" "" False
    else AddPasswordInput "" "" False

setAddNewPassword : PasswordState -> NewPassword -> PasswordState
setAddNewPassword state s =
    case state of
    AddPasswordInput _ pr a -> AddPasswordInput s pr a
    _ -> state

setAddNewPasswordRepeat : PasswordState -> NewPasswordRepeat -> PasswordState
setAddNewPasswordRepeat state s =
    case state of
    AddPasswordInput np _ a -> AddPasswordInput np s a
    _ -> state

setChangeCurrentPwd : PasswordState -> CurrentPassword -> PasswordState
setChangeCurrentPwd state s =
    case state of
    ChangePasswordInput _ np pr a -> ChangePasswordInput s np pr a
    _ -> state

setChangeNewPwd : PasswordState -> NewPassword -> PasswordState
setChangeNewPwd state s =
    case state of
    ChangePasswordInput cp _ pr a -> ChangePasswordInput cp s pr a
    _ -> state

setChangePwdRepeat : PasswordState -> NewPasswordRepeat -> PasswordState
setChangePwdRepeat state s =
    case state of
    ChangePasswordInput cp np _ a -> ChangePasswordInput cp np s a
    _ -> state

type alias OriginalName = String
type alias NewName = String

type NameState 
    = NameUnavailiable
    | NameInput OriginalName NewName Bool
    | NamePending
    | NameError

initNameState : NameState
initNameState = NameUnavailiable

profileNamestate : Profile -> NameState
profileNamestate p =
    NameInput p.name "" False

setNewNameInput : NameState -> NewName -> NameState
setNewNameInput state s =
    case state of
    NameInput on _ a -> NameInput on s a
    _ -> state

type alias AntiCsrfToken = String

type alias Model =
    { antiCsrf : AntiCsrfToken
    , profile : ApiResponse Profile
    , password : PasswordState
    , name: NameState
    , oidcLogin : Maybe OidcLogin
    }

refresh : Profile -> Model -> Model
refresh p m =
    { antiCsrf = m.antiCsrf
    , password = profilePasswordState p
    , name = profileNamestate p
    , profile = Received p
    , oidcLogin = m.oidcLogin
    }

type Msg =
    Noop
    | GotProfile (Result Error Profile)
    | ChangePasswordState PasswordState
    | ChangeNameState NameState
    | AddPwdNew String
    | AddPwdRepeat String
    | AddPwdSubmit
    | AddPwdResponse (Result Error ())
    | ChangePwdCurrent String
    | ChangePwdNew String
    | ChangePwdRepeat String
    | ChangePwdSubmit 
    | ChangePwdResponse (Result Error ())
    | ChangeNameNew String
    | ChangeNameSubmit
    | ChangeNameResponse (Result Error ())

fetchProfile : Cmd Msg
fetchProfile =
    Http.get
    { url = UrlB.absolute ["api", "profile"] []
    , expect = Http.expectJson GotProfile decodeProfile
    }

init : Flags -> (Model, Cmd Msg)
init f = 
    ( { antiCsrf = f.antiCsrf 
      , profile = Pending
      , password = initPasswordState
      , name = initNameState
      , oidcLogin = f.oidcLogin
      }
    , fetchProfile )

renderError : List (Html msg)
renderError =
    [ Html.h2 [] [ Html.text "Error" ]
    , Html.p [] [ Html.text "You can try reloading the page."]
    ]

spinner : Html Msg
spinner =
    Html.div [ class "progress" ] 
    [ Html.div [ class "indeterminate" ] []
    ]

view : Model -> Document Msg
view m = 
    { title = "Profile"
    , body = 
        [ Html.div [ class "container" ] 
          [ Html.div [ class "row" ]
            [ Html.div [ class "col s12 m6" ] 
                (Html.h1 [] [ Html.text "Profile" ]
                :: (case m.profile of
                    Pending -> [ Html.text "" ]
                    Errored -> renderError 
                    Received p -> renderProfile m.oidcLogin p m.password m.name ))
            ]  
          ] 
        ]
    }
        
update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        Noop ->
            (model, Cmd.none)

        GotProfile (Ok p) ->
            ( refresh p model
            , Cmd.none
            )

        GotProfile (Err _) ->
            ( { model | profile = Errored }
            , Cmd.none 
            )

        ChangePasswordState s ->
            ( { model | password = s }
            , Cmd.none 
            )

        ChangeNameState s ->
            ( { model | name = s }
            , Cmd.none 
            )

        AddPwdNew s -> 
            ( { model | password = setAddNewPassword model.password s }
            , Cmd.none
            )

        AddPwdRepeat s -> 
            ( { model | password = setAddNewPasswordRepeat model.password s }
            , Cmd.none
            )

        AddPwdSubmit -> 
            ( { model | password = PasswordPending }
            , submitAddPassword model.antiCsrf model.password
            )

        AddPwdResponse (Ok _) -> 
            ( { model | password = PasswordUnavailiable }
            , fetchProfile
            )

        AddPwdResponse (Err _) ->
            ( { model | password = PasswordError }
            , Cmd.none
            )

        ChangePwdCurrent s -> 
            ( { model | password = setChangeCurrentPwd model.password s }
            , Cmd.none
            )

        ChangePwdNew s -> 
            ( { model | password = setChangeNewPwd model.password s }
            , Cmd.none
            )

        ChangePwdRepeat s -> 
            ( { model | password = setChangePwdRepeat model.password s }
            , Cmd.none
            )

        ChangePwdSubmit -> 
            ( { model | password = PasswordPending }
            , submitChangePassword model.antiCsrf model.password
            )

        ChangePwdResponse (Ok _) -> 
            ( { model | password = PasswordUnavailiable }
            , fetchProfile
            )

        ChangePwdResponse (Err _) ->
            ( { model | password = PasswordError }
            , Cmd.none
            )

        ChangeNameNew s -> 
            ( { model | name = setNewNameInput model.name s }
            , Cmd.none
            )

        ChangeNameResponse (Ok _) -> 
            ( { model | name = NameUnavailiable }
            , fetchProfile
            )

        ChangeNameResponse (Err _) ->
            ( { model | name = NameError }
            , Cmd.none
            )

        ChangeNameSubmit -> 
            ( { model | name = NamePending }
            , submitChangeName model.antiCsrf model.name
            )
        

subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none

main : Program Flags Model Msg
main = Browser.document
    { init = init
    , view = view
    , update = update
    , subscriptions = subscriptions
    }