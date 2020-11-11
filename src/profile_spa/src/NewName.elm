module NewName exposing (Model, Msg, init, view)

import Http exposing (Error, Header)
import Html exposing (Html)

type alias NameInput = String

isDirty : NameInput -> Bool
isDirty i = i /= ""

type ApiStatus
    = CollectingInput
    | Pending
    | Errored
    | Submitted

type alias Model = (ApiStatus, NameInput)

init : Model
init = (CollectingInput, "")

type Msg
    = NameInputChanged String
    | SubmitName
    | SubmitNameResponse (Result Error ())
    | Noop

view : List (Html msg)
view = [ Html.h5 [] [ Html.text "Add name"] ]