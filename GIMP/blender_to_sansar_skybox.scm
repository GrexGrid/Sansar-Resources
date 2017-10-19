;; This program is free software: you can redistribute it and/or modify
;; it under the terms of the GNU General Public License as published by
;; the Free Software Foundation, either version 3 of the License, or
;; (at your option) any later version.
;;
;; This program is distributed in the hope that it will be useful,
;; but WITHOUT ANY WARRANTY; without even the implied warranty of
;; MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;; GNU General Public License for more details.

;; To get a copy of the GNU General Public License see
;; <http://www.gnu.org/licenses/>.

( define
    ( script-fu-blender-to-sansar-skybox orig_img drawable )

    ( let*
        (
            ( layers ( gimp-image-get-layers orig_img ) )
            ( num_layers ( car layers ) )
            ( layer_array ( cadr layers ) )
            ( bottom_layer ( aref layer_array ( - num_layers 1 ) ) )
            ( sel-float 0 )
            ( copy_x 0 )
            ( copy_y 0 )
            ( paste_x 0 )
            ( paste_y 0 )

            ;; The size used for the layers are based on the first layer
            ( orig_layer_w ( car ( gimp-drawable-width bottom_layer ) ) )
            ( orig_layer_h ( car ( gimp-drawable-height bottom_layer ) ) )
            ;; blender cubemaps are 3 x 2, get the width and divide by 3
            ( box_w ( / orig_layer_w  3) )
            ;; sansar skyboxes are 6 x 1
            (new_image_w (* box_w 6))
            (new_image_h (* box_w 1))

            ;; Create image
            ( new_img ( car ( gimp-image-new new_image_w new_image_h RGB ) ) )

            ( new_layer
                ( car
                    ( gimp-layer-new
                        new_img
                        new_image_w
                        new_image_h
                        RGB-IMAGE
                        "Sansar Skybox"
                        100
                        NORMAL-MODE
                    )
                )
            )
        )

        ( gimp-image-undo-disable new_img )
        ( gimp-image-add-layer new_img new_layer 0 )

        ;; right front left
        ;; bottom top back
        ;; to
        ;; left right top bottom back front
        ;; 0 1 2 3 4 5

        ;;right
        (set! copy_x (* box_w 2))
        (set! copy_y (* box_w 0))
        (set! paste_x (* box_w 0))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float) 

        ;;front
        (set! copy_x (* box_w 1))
        (set! copy_y (* box_w 0))
        (set! paste_x (* box_w 5))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float)

        ;;left
        (set! copy_x (* box_w 0))
        (set! copy_y (* box_w 0))
        (set! paste_x (* box_w 1))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float)

        ;;bottom
        (set! copy_x (* box_w 0))
        (set! copy_y (* box_w 1))
        (set! paste_x (* box_w 3))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float)

        ;;top
        (set! copy_x (* box_w 1))
        (set! copy_y (* box_w 1))
        (set! paste_x (* box_w 2))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float)

        ;;back
        (set! copy_x (* box_w 2))
        (set! copy_y (* box_w 1))
        (set! paste_x (* box_w 4))
        (set! paste_y (* box_w 0))
        ( gimp-rect-select orig_img copy_x copy_y box_w box_w CHANNEL-OP-REPLACE FALSE 0 )
        (gimp-edit-copy drawable)
        (set! sel-float (car (gimp-edit-paste new_layer FALSE)))
        (gimp-layer-set-offsets sel-float paste_x paste_y)
        (gimp-floating-sel-anchor sel-float)

        ( gimp-image-undo-enable new_img )
        ( gimp-image-clean-all new_img)
        ( gimp-display-new new_img )
    )

    ( gimp-image-undo-thaw orig_img )
    ( gimp-displays-flush )
)

( script-fu-register
  "script-fu-blender-to-sansar-skybox"
  "Blender to Sansar Skybox" 
  "Creates a new image with the Sansar Skybox Format"
  "OldVamp"
  "Copyright 2017, OldVamp"
  "Oct 19, 2017"
  ""
  SF-IMAGE    "Image"                0
  SF-DRAWABLE "Drawable"             0
) 
( script-fu-menu-register "script-fu-blender-to-sansar-skybox" "<Image>/Filters/Sansar" )
