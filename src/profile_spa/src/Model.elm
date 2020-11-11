module Model exposing (Profile)

type alias Profile = 
    { name : String
    , email : String
    , hasPassword : Bool
    }
