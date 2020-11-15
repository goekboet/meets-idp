module Components exposing (StepStatus(..), ChangeStatus(..), completionStep, completionProgress, input, Input, emailCompletion)

import Html exposing (Html)
import Html.Attributes as Attr exposing (class, disabled)
import Html.Events exposing (onClick, onInput)
import Html.Attributes exposing (classList)
import Html.Attributes exposing (type_)
import Html.Attributes exposing (tabindex)
import Html.Attributes exposing (autofocus)

type alias InputId = String
type alias InputType = String
type alias IconId = String
type alias InputLabel = String

type alias Input =
    { id : InputId
    , type_ : InputType
    , icon : IconId
    , label : InputLabel
    , tabindex : Int
    , autofocus : Bool
    }

type alias InputDirty = Bool
type alias InputValidation = String



input : Input -> InputDirty -> InputValidation -> (String -> msg) -> Html msg
input { id, type_, icon, label, tabindex, autofocus } dirty validation msg 
    =  
    Html.div [ class "input-field" ] 
    [ Html.i [ class "material-icons prefix" ] [ Html.text icon ]
    , Html.input 
        [ class "validate"
        , Attr.placeholder label
        , Attr.name id
        , Attr.type_ type_
        , Attr.tabindex tabindex
        , Attr.autofocus autofocus
        , Attr.autocomplete False
        , onInput msg ] 
        []
    , Html.label [ class "active", Attr.for id ] [ Html.text (label ++ (if dirty then "*" else ""))]
    , Html.span [ class "helper-text" ] [ Html.text validation ]
    ]

type StepStatus
    = Complete
    | Current
    | InComplete

type alias StepName = String

type alias StepOutcome = Maybe String

stepIcon : StepStatus -> Html msg
stepIcon status =
    let
        icon s = Html.i [ class "material-icons" ] [ Html.text s ]
    in
    case status of
        Complete -> icon "check_box"
        Current -> icon "chevron_right"
        InComplete -> icon "check_box_outline_blank"

type ChangeStatus
    = NotApplicable
    | Changing
    | Applicable

changeButton : ChangeStatus -> msg -> Html msg
changeButton status m =
    case status of
    NotApplicable -> Html.button [ class "btn-flat", Attr.style "visibility" "hidden" ] [ Html.text "Change" ]
    Changing -> Html.button [ class "btn-flat", Attr.disabled True ] [ Html.text "Change" ]
    Applicable -> Html.button [ class "btn-flat", onClick m ] [ Html.text "Change" ]

stepName : StepName -> Html msg
stepName name = Html.label [ class "name" ] [ Html.text name ]

stepOutcome : StepOutcome -> StepStatus -> Html msg
stepOutcome outcome status =
    let
        labelText o = Maybe.map Html.text o |> Maybe.withDefault (Html.text "")
        style s =
            case s of
            Current -> [ ("status", True), ("black-text", True) ]
            _ -> [ ("status", True), ("black-text", False) ]
    in
        Html.label [ classList (style status), Attr.style "flex-grow" "1" ] [ labelText outcome ]
    

completionStep : StepStatus -> ChangeStatus -> msg -> StepName -> StepOutcome -> List (Html msg)
completionStep stepS changeS m stepN stepO =
    [ stepIcon stepS
    , stepName stepN
    , stepOutcome stepO stepS
    , changeButton changeS m
    ]

emailCompletion : String -> ChangeStatus -> List (Html msg)
emailCompletion email cs =
    let
        visibility = 
            case cs of
            NotApplicable -> Attr.style "visibility" "hidden"
            _ -> Attr.style "visibility" "visible"

    in
    
    [ stepIcon Complete
    , stepName "Email"
    , Html.label [ Attr.style "flex-grow" "1", class "status truncate" ] [ Html.text (email ++ " (verified)") ]
    , Html.a [ Attr.href "/ChangeUsername", class "btn-flat", visibility ] [ Html.text "Change" ]
    ]

completionProgress : List (Html msg) -> List (Html msg) -> List (Html msg) -> List (Html msg) -> Maybe (List (Html msg)) -> Html msg
completionProgress email password name complete delete =
    let
        listitem i = Html.li [ class "collection-item" ] i 
    in
    Html.ul [ class "collection profile-completion" ] 
    [ listitem email
    , listitem password 
    , listitem name 
    , listitem complete 
    , Maybe.map listitem delete |> Maybe.withDefault (Html.text "")
    ] 