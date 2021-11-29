module Footer

open Feliz
open Style

[<ReactComponent>]
let Footer() =
        
    let copywrite =
        $"""©ElmishAADB2C 2017-{System.DateTime.UtcNow.ToString("yyyy")}"""
    Html.div[
        Html.footer [
            prop.classes [  css.``py-4``
                            css.``bg-light``
                            css.``mt-auto``
                            css.``d-print-none``] 
            prop.children [
                Html.div [
                    prop.className css.``container-fluid``
                    prop.children [
                        Html.div [
                            prop.classes [css.``d-flex``; css.``justify-content-center``]
                            prop.children[                                    
                                Html.div [
                                    prop.className css.``text-muted``
                                    prop.text copywrite
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]


