module Main exposing (main)

import Browser exposing (Document)
import Html
import Html.Attributes exposing (class)

type alias Flags =
    { antiCsrf : String }

type alias Model =
    { key1 : String
    , key2 : String
    }

type Msg =
    Noop
    | DoIt String

init : Flags -> (Model, Cmd msg)
init _ = ({ key1 = "", key2 = ""}, Cmd.none )

-- layout : 

view : Model -> Document msg
view m = 
    { title = "Some title"
    , body = 
        [ Html.div [ class "root-view" ] 
          [ Html.div [ class "content heavy" ] 
            [ Html.h1 [] [ Html.text "Profile" ]
            ]  
          , Html.div [ class "content light" ] 
            [ Html.h2 [] [ Html.text "Username" ]
            ] 
          ] 
        ]
    }
        

update : Msg -> Model -> (Model, Cmd msg)
update msg model =
    case msg of
        Noop ->
            (model, Cmd.none)

        DoIt _ ->
            (model, Cmd.none)

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