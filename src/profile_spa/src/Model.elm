module Model exposing (Profile, isComplete)

type alias Profile = 
    { name : String
    , email : String
    , hasPassword : Bool
    }

isComplete : Profile -> Bool
isComplete p = p.name /= "" && p.hasPassword
