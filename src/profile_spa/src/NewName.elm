module NewName exposing (Model, Msg, init, view, update)

import Http exposing (Error, Header)
import Html exposing (Html)
import Components exposing (StepStatus(..), ChangeStatus(..), completionStep, completionProgress, input, Input)
import Html.Attributes as Attr exposing (class, disabled)
import Html.Events exposing (onClick, onInput)
import Url.Builder as UrlB
import Json.Encode as Encode exposing (Value)
import Model exposing (Profile, isComplete)

type alias NameInput = String

isDirty : NameInput -> Bool
isDirty i = i /= ""

type ApiStatus
    = CollectingInput
    | Pending
    | Errored
    | Submitted

apiStatusRejection : ApiStatus -> Rejection
apiStatusRejection s =
    case s of
    Errored -> "This name was not accepted. Try another one."
    _ -> ""

type alias Model = (ApiStatus, NameInput)

init : Model
init = (CollectingInput, "")

type Msg
    = NameInputChanged String
    | SubmitName
    | SubmitNameResponse (Result Error ())
    | Noop

inCompleteStatus : (Msg -> msg) -> Profile -> Model -> Html msg
inCompleteStatus toApp p m =
    completionProgress
        (completionStep Complete NotApplicable (toApp Noop) "Email" (Just (p.email ++ " (verified)")))
        (completionStep Complete NotApplicable (toApp Noop) "Password" (Just "added"))
        (completionStep Current NotApplicable (toApp Noop) "Name" (Just "not added"))
        (completionStep InComplete NotApplicable (toApp Noop) "Profile complete" Nothing)
        Nothing

completeStatus : (Msg -> msg) -> Profile -> Model -> Html msg
completeStatus toApp p m =
    completionProgress
        (completionStep Complete NotApplicable (toApp Noop) "Email" (Just (p.email ++ " (verified)")))
        (completionStep Complete NotApplicable (toApp Noop) "Password" (Just "added"))
        (completionStep Complete Changing (toApp Noop) "Name" (Just p.name))
        (completionStep Complete NotApplicable (toApp Noop) "Profile complete" Nothing)
        Nothing

type alias AntiCsrfToken = String

update : AntiCsrfToken -> (Msg -> msg) -> Msg -> Model -> (Model, Cmd msg, String)
update anticsrf toApp msg (api, inp) =
    case msg of
        NameInputChanged s -> 
            ((api, s), Cmd.none, "")
        SubmitName -> 
            ((Pending, inp), changeNameApiCall anticsrf  toApp inp, "" )
        SubmitNameResponse (Ok ()) -> ((Submitted, inp), Cmd.none, inp )
        SubmitNameResponse (Err e) -> ((Errored, inp), Cmd.none, "" )
        Noop -> ((api, inp), Cmd.none, "" )

nameInput : Input
nameInput =
    { id = "NewName"
    , type_ = "text"
    , icon = "person_outline"
    , label = "Name"
    , tabindex = 1
    , autofocus = True
    }

antiCsrfHeader : String ->  Header
antiCsrfHeader t =
    Http.header "RequestVerificationToken" t

nameJson : NameInput -> Value
nameJson n =
    Encode.object [ ("name", Encode.string n) ]

changeNameApiCall : AntiCsrfToken -> (Msg -> msg) -> NameInput -> Cmd msg
changeNameApiCall t toApp nn =
    Http.request
    { method = "post"
    , headers = [ antiCsrfHeader t ]
    , url = UrlB.absolute ["api", "profile", "name" ] []
    , body = nameJson nn |> Http.jsonBody
    , expect = Http.expectWhatever (toApp << SubmitNameResponse)
    , timeout = Nothing
    , tracker = Nothing
    }

type alias Rejection = String

nameRejection : Model -> Rejection
nameRejection (api, n) =
    if (String.length n == 0) then "Name must not be empty."
    else if (String.length n > 256) then "Name must not be longer than 256 characters."
    else ""

addNameForm : (Msg -> msg) -> Model -> Html msg
addNameForm toApp (api, inp) =
    let
        nameReject = nameRejection (api, inp)
        rejected = 
            nameReject 
            |> String.isEmpty
            |> not

        pending = 
            case api of
            Pending -> True
            _ -> False
    in
    
    Html.div [ class "section" ] 
    [ input 
        nameInput 
        (isDirty inp) 
        nameReject 
        (toApp << NameInputChanged) 
    , Html.button 
      [ class "btn"
      , Attr.name "SubmitName"
      , onClick (toApp SubmitName)
      , Attr.disabled (rejected || pending)
      ] 
      [ Html.text "Submit" ]
    ]

view : (Msg -> msg) -> Profile -> Model -> List (Html msg)
view toApp p m = 
    [ Html.h5 [] [ Html.text "Choose a name"]
    , if isComplete p
      then completeStatus toApp p m
      else inCompleteStatus toApp p m 
    , Html.p [] [ Html.text "This is the name other users see. If you want to be anonymous we encourage you to use \"n/a\"."]
    , addNameForm toApp m  
    ]