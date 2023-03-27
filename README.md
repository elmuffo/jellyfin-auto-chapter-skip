# Auto Chapter Skip

A plugin for [Jellyfin](https://jellyfin.org/docs/) to automatically skip chapters based on the chapter name matching a [regular expression](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)

## Features

* Automatically skip 1 or more chapters in series with a name matching a regular expression
* Allows the user to manually seek backwards on a video to watch a previously skipped chapter, the plugin will only resume skipping chapters after playback returns to a previously unwatched position
* Stops video playback if all remaining chapters are to be skipped and marks the video as watched

## Supported Clients
* [Jellyfin Web](https://github.com/jellyfin/jellyfin-web)
* [Jellyfin Android TV](https://github.com/jellyfin/jellyfin-androidtv)
* [Jellyfin Android](https://github.com/jellyfin/jellyfin-android)
* [JellyCon](https://github.com/jellyfin/jellycon)

## Installation

Add a new plugin repository to Jellyfin and use https://github.com/elmuffo/jellyfin-auto-chapter-skip/blob/master/manifest.json for the Repository URL, then install Auto Chapter Skip on the Catalog tab and restart Jellyfin to complete the process

## Setup

Navigate to the plugin settings and configure a regular expression to match on. Upon first installation the match field is empty, this will not match anything and no chapters will be skipped.

#### Examples

* Match: `Intro`
    * Skip all chapters that contain the word Intro
* Match: `\[SponsorBlock\]: Sponsor`
    * Skip chapters that are Sponsor segments as marked by SponsorBlock

## FAQ

**Q: Does this work on chromecast?**

**A:** Unfortunately there is no support for sending `PlaystateCommand.Seek` to the Chromecast client

**Q: Does this work on other clients?**

**A:** I'm unable to verify other clients as I don't have access to those devices. Please let me know if you are successful

**Q: How does it work?**

**A:** This plugin attaches an event listener to the `ISessionManager.PlaybackProgress` event, if the current playback position is inside the bounds of a chapter that we want to skip, we send a `PlaystateCommand.Seek` with a target position of the start of the next chapter that doesn't match the regular expression. All plugin data held in memory related to a `Session` is removed once playback is stopped to keep the memory footprint low.
