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
import Model exposing (Profile)
import NewPassword
import NewName
import ChangePassword
import Components exposing (StepStatus(..), ChangeStatus(..), completionStep, completionProgress, input, Input)
import ChangePassword

decodeProfile : Decoder Profile
decodeProfile =
    Decode.map3 Profile
        (Decode.field "name" Decode.string)
        (Decode.field "email" Decode.string)
        (Decode.field "hasPassword" Decode.bool)
    
type alias AntiCsrfToken = String

type alias OidcLogin =
    { id : String
    , name : String
    }

type Msg 
    = Noop
    | GotProfile (Result Error Profile)
    | ChangePassword Profile
    | NewPwdMsg NewPassword.Msg
    | NewNameMsg NewName.Msg 
    | ChangePwdMsg ChangePassword.Msg

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
    Noop -> (model, Cmd.none)
    GotProfile (Ok p) ->
        ( { model | profileCompletion = getProfileCompletion p }
        , Cmd.none
        )

    GotProfile (Err _) ->
        ( { model | faulty = True }
        , Cmd.none 
        )

    ChangePassword p ->
        ( { model | profileCompletion = ChangingPassword p ChangePassword.init}, Cmd.none)

    NewPwdMsg m -> 
        case model.profileCompletion of
        AddingPassword profile newPwdModel -> 
            let
                (newPwdModel_, cmd, done) = 
                    NewPassword.update
                        model.antiCsrf
                        NewPwdMsg 
                        m 
                        newPwdModel

                nextStep = 
                    if done 
                    then getProfileCompletion { profile | hasPassword = True } 
                    else AddingPassword profile newPwdModel_
            in 
                (
                  { model | profileCompletion = nextStep }
                , cmd 
                )
    
        _ -> (model, Cmd.none)
    
    NewNameMsg m ->
        case model.profileCompletion of
        AddingName profile newNameModel -> 
            let
                (newNameModel_, cmd, name) = 
                    NewName.update
                        model.antiCsrf
                        NewNameMsg 
                        m 
                        newNameModel

                nextStep = 
                    if name /= "" 
                    then getProfileCompletion { profile | name = name } 
                    else AddingName profile newNameModel_
            in 
                (
                  { model | profileCompletion = nextStep }
                , cmd 
                )
    
        _ -> (model, Cmd.none)

    ChangePwdMsg m -> 
        case model.profileCompletion of
        ChangingPassword profile changePwdModel -> 
            let
                (changePwdModel_, cmd, done) = 
                    ChangePassword.update
                        model.antiCsrf
                        ChangePwdMsg 
                        m 
                        changePwdModel

                nextStep = 
                    if done 
                    then getProfileCompletion { profile | hasPassword = True } 
                    else ChangingPassword profile changePwdModel_
            in 
                (
                  { model | profileCompletion = nextStep }
                , cmd 
                )
    
        _ -> (model, Cmd.none)


type ProfileCompletion 
    = Uninitialized
    | AddingPassword Profile NewPassword.Model
    | AddingName Profile NewName.Model
    | ProfileComplete Profile
    | ChangingPassword Profile ChangePassword.Model

getProfileCompletion : Profile -> ProfileCompletion
getProfileCompletion p =
    case (p.name, p.hasPassword) of
    (_, False)    -> AddingPassword p NewPassword.init
    ("", _) -> AddingName p NewName.init
    _          -> ProfileComplete p

type alias Model =
    { antiCsrf : AntiCsrfToken
    , oidcLogin : Maybe OidcLogin
    , profileCompletion : ProfileCompletion
    , faulty : Bool
    }

type alias Flags =
    { antiCsrf : String 
    , oidcLogin : Maybe OidcLogin
    }

init : Flags -> (Model, Cmd Msg) 
init f = 
    (
      { antiCsrf = f.antiCsrf
      , oidcLogin = f.oidcLogin 
      , profileCompletion = Uninitialized
      , faulty = False
      }
    , fetchProfile
    )

content : Model -> List (Html Msg)
content model =
    case model.profileCompletion of
    Uninitialized -> []
    AddingPassword profile addPwdModel -> 
        NewPassword.view NewPwdMsg profile.email addPwdModel
    AddingName profile newNameModel ->
        NewName.view NewNameMsg profile.email newNameModel
    ProfileComplete profile ->
        completeView profile model
    ChangingPassword profile changePwdModel ->
        ChangePassword.view ChangePwdMsg profile changePwdModel

oidcLogin : Model -> Html Msg
oidcLogin m =
    case m.oidcLogin of
    Just login ->  Html.a [ class "btn", Attr.href login.id ] [ Html.text ("Login") ]
    _ -> Html.text ""

completionStatus : Profile -> Model -> Html Msg
completionStatus p m =
    completionProgress
        (completionStep Complete Applicable Noop "Email" (Just (p.email ++ " (verified)")))
        (completionStep Complete Applicable (ChangePassword p) "Password" (Just "added"))
        (completionStep Complete Applicable Noop "Name" (Just p.name ))
        (completionStep Complete NotApplicable Noop "Profile complete" Nothing)
        Nothing

completeView : Profile -> Model -> List (Html Msg)
completeView p m = 
    [ Html.h5 [] 
        [ Html.text "Profile complete"]
    , completionStatus p m  
    , oidcLogin m
    ]

view : Model -> Document Msg
view m = 
    { title = "Profile"
    , body = 
        [ Html.div [ class "container"] 
          [ Html.div [ class "row" ] 
              [ Html.div [ class "col s12 m6" ] (Html.h1 [] [ Html.text "Profile" ] :: content m) ] 
          ]
        ] 
    }

fetchProfile : Cmd Msg
fetchProfile =
    Http.get
    { url = UrlB.absolute ["api", "profile"] []
    , expect = Http.expectJson GotProfile decodeProfile
    }

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