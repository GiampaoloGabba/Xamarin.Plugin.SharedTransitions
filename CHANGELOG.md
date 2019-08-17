## 2.0.2
* Fix issue [#10](https://github.com/Evolutionlab/Xamarin.Plugin.SharedTransitions/issues/10) (crash in iOS with non-existant image)
* Fix issue [#11](https://github.com/Evolutionlab/Xamarin.Plugin.SharedTransitions/issues/11) (backward transition duration in iOS)
* Fix Android backward transition duration

## 2.0.1
* **BREAKING:** Properties renamed to: Transition.Name, Transition.Group and TransitionDuration 
* Full MVVM support with TransitionSelectedGroup
* New, improved Transition management under the hood (no more limitation to int number for transition names & groups, faster view lookup for transitions)
* New transition engine for ios (support for shape transitions, including boxview, images with different bounds, aspect ratios, ecc..)
* New sample app in full MVVM with listview, collectionview and normal transitions (including demostrations with PancakeView and FFImageLoading)
* Everything mostly rewritten fro the ground-up: better stability and functionality
* Improved code comments and error handling 
* Added useful notes in code to help contributors to make this plugin better!


## 1.1.0
* Update to .NETStandard 2
* Fixed transitions between images with different aspect ratios
* Enable transitions between layouts (frame, stacklayout, ecc..) *Note: BoxView in iOS is not currently supported in this version*
* Updated Xamarin.Forms version from 3.1 to 3.6 (#6) by **@jsuarezruiz**
* Added new BackgroundAnimations (Flip, SlideFromLeft, etc.) (#7) by **@jsuarezruiz**
* Updated README with a new sample reference (#8) by **@jsuarezruiz**


## 1.0.0
* First release