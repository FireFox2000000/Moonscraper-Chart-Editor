---
layout: post
title: "Why Moonscraper Chart Editor 2?"
author:
  - FireFox
---


Tech debt. 

…

That’s it, post over. 

…

…

…

You want more? Okay fine. But only because you asked nicely.

For those who may have missed it, Moonscraper Chart Editor 2 was teased last year as a little Christmas present to the community. So yes, Moonscraper 2 confirmed:

{% include youtube.html id="iaLBjju17cs" %}

But what was not clear was that this is actually a complete rebuild of Moonscraper from the ground up, which is why MSCE1 hasn’t been getting any updates for ages. But why would I go ahead and ditch everything to start from scratch?

A few years ago I was sent a random message on Discord, and surprisingly it wasn’t a scam! It was actually someone who introduced themselves as someone from the Lanota community. [Lanota](https://youtu.be/VqzPE0dCFKo?si=HVOEPqtXD9Ui_uRE), if you aren’t aware, is a rhythm game for Android and Switch that plays out similar to Tap Tap, but the notes come at you from a circular highway, kind of like a mix of Guitar Hero and Maimai. I personally tried it out on Switch. Fun game, can recommend. 

Anyways, this person messaged me with a request. They wanted to commission me to make a new map editor for their game, as the current tools they were working with were supposedly difficult to use and falling apart a bit, and were impressed by the ease-of-use Moonscraper Chart Editor offered for Guitar Hero game charting. Despite being a cool opportunity and something I very much would have loved to delve into, unfortunately I did end up declining the offer, as the amount of work required was estimated to be too much for the amount of spare time I had unless they were somehow able to offer proper freelance prices (which would have been completely unreasonable for the budget they were working with). I did very much appreciate the offer however, and it started turning some gears in my head. Not long afterwards, a question popped into my head: could Moonscraper be repurposed to make a Lanota map editor and just do the request for free?

Pfft, of course not. Unfortunately Moonscraper was way too purpose built for the Guitar Hero game-style, and thus poorly architectured for such things from the start (ah, the joys of being a young and clueless dev). Hell, it wasn’t even built for Clone Hero and YARG! It was initially built as a replacement for [FeedBack](https://github.com/TurkeyMan/feedback-editor) (which in itself was built to be a Guitar Hero 2 editor) and to support the work [Exilelord was doing on GH3+](https://www.youtube.com/playlist?list=PLNeWBe23yMsoO3Emyz3c7o1zzu5geRVNR). All the support for multiple instruments and additional features CH added is held together in Moonscraper by duct tape and pixie dust. And I’m not even sure the duct tape is still there.

However, that did raise the question, what would a Moonscraper that could display any kind of rhythm game look like? In addition to that, there have been a number of really cool feature requests I’ve gotten and always wanted to implement, such as editing multiple tracks at once, making windows moveable, having a proper Guitar Hero: Live view for GHL tracks, vocals editor etc, that just couldn’t be done without untangling a bunch of code spaghetti. There were also a few long term bugs that to solve would have been to tear up a core fundamental part of how Moonscraper works (yes, this is the reason that Moonscraper literally starts disintegrating when making extremely long charts). 

At this point I’ve been working in the games industry as a professional programmer for a number of years, and am a far, far better developer than what I was when I first started working on Moonscraper. It was at that point that a little devil appeared on my shoulder to tell me “hey, make a prototype, you should see if this is actually feasible”.

And so after one weekend in full game jam mode I had this:

{% include youtube.html id="EiHnghPOWM4" %}



3 different tracks, 3 different game visuals, 1 song. Ah fuck, this is feasible isn’t it?

Let’s take a look at this in some closer detail!

On the bottom left is a traditional Guitar Hero track layout, playing out the Expert Guitar track of Black Widow of La Porte. However this was also an experiment for mod-chart animations with the mexican wave animation that plays intermittently (note that modchart support is super unlikely for MSCE2, this is just proving out the concept that the engine could handle it).

Next on the right-hand side is a Stepmania inspired layout playing out the Expert Drums track of the same song. This was a test for handling rendering of a completely different set of note sprites, as well as per-lane independent animations (take note of the outer-most lane swapping back and forth between the two sides).

Finally we have a super experimental Rocksmith/Lanota inspired highway playing the Expert Bass track (again, no plans for Rocksmith support anytime soon, too much else to do at the moment). This was also a test for full 3d rendering, using splines to denote each lane the position through 3d space. Each “hit” marker is also slightly offset from the others, allowing for different “perfect” hits to have different visual positions.

And that was it, Moonscraper 2 was officially in development from that point forward. 

The keen eyed among you may have noticed the timestamp at which this was uploaded, which was many, many moons ago. Hell, it’s even been 8 months since the teaser was uploaded and made public. Alas, development has been at a much slower pace compared to the cycle of the original Moonscraper, and this is purely due to having far less free time given different life circumstances. Moonscraper 1 was made after I finished University as something to keep myself busy while job hunting, and also ended up as a portfolio piece for my resume, which did actually land me my first job in game dev! Moonscraper 2 however is actively being developed while I’m making other games in a professional capacity, so it’s a drastic reduction in free time to put towards this (damn the requirement to need to afford things and needing to not burn myself out). That being said, development is actually pretty steady and given the actual amount of time I’ve put into development I’m very happy with what I’ve got cooking so far. 

That being said, I can’t give out any estimates on when Moonscraper 2 will be in a state that I’d be happy to publicly release it and replace Moonscraper 1. I’m currently solo-deving it at the moment, which goes a lot slower than working with a team. Originally I wanted to bring other people onto the project to help out, but it’s difficult to convince people to work on an editor when the allure of working on an actual game like YARG or Fret Smasher is so much more appealing. There’s still a few features to back-support from MSCE1, as well as new features getting prototyped and refined out. Yes, it’s a lot more than just being able to edit multiple windows at once, as I’m basically taking the approach of how I would make and design a Chart Editor given years more development experience and I keep getting more ideas on things I want to improve. The literal definition of scope creep. Send help.
So far I think almost every system has gotten a full rewrite: audio systems are a lot more robust with synchronisation improvements (no longer need the separated SFX calibration, which was a hackfix on itself), UX improvements, and even bugfixes that I didn’t even realise MS1 has (fun fact that MCES1’s waveform rendering is very incorrect, woo!). 

A major focus from the start was for performance improvements as well, with Exilelord’s Crash Test 5 chart being set as a benchmark. Moonscraper 1 could barely hold a framerate when rendering out this chart as soon as it got even slightly intense. Moonscraper 2 can render that same instance many, many times over while still holding a playable framerate (this is specific to my machine, mileage will vary based on the final release and your own machine’s capabilities):

{% include youtube.html id="FPCKPxYm9ts" %}



And that’s it so far. Well, so far with what I actually want to show, gotta keep some cards close to the chest. I can’t say that a Lanota editor will ever be built, especially with Clone Hero, YARG and Stage Tour ramping interest back up into the GH community and into the mainstream for Guitar Hero-style games again. But maybe one day, it’ll certainly stick in the back of my mind. Maybe a fork of MS2 could exist for someone else to take the reins on that? Though I’m still considering whether MSCE2 should even be open source or not, as MSCE1 being open source had both positives and negatives. Might consider open sourcing only parts of MSCE2? License it out non-commercially only? Who knows really, I need to focus on actually making the damn thing first. I hope you look forward to what I have in store in the future. ⸜(｡˃ ᵕ ˂ )⸝
