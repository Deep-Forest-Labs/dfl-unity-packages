# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.9] - 2023-01-19
 - Exposed the `Format` method.

## [1.0.8] - 2023-01-19
 - Removed `Handled` error and exception logging.  Only trigger the event.
 - Updated `HandleErrorLogged` to send `format` and `args`

## [1.0.7] - 2023-01-19
 - Remove `errorEncountered` support
 - Set `DebugError` to send `HandledError` on `PRODUCTION_BUILD`

## [1.0.6] - 2023-01-19
 - Remove `Log.Exception` support and replaced with `Log.HandledException` that will not cause `errorEncountered` and will log as a warning.
 - Added `Log.HandledError` which will not cause `errorEncountered` and will log as a warning.

## [1.0.5] - 2022-11-08
 - Added `errorEncountered` support

## [1.0.4] - 2022-10-13
 - Added `DebugError`

## [1.0.3] - 2021-08-27
 - Fixed issue with the ZString Format call.

## [1.0.2] - 2021-08-26
 - Converted string to ZString calls.

## [1.0.1] - 2021-04-02
 - Changed the namespace from `Common.Utils` to `Common.Logger`

## [1.0.0] - 2021-03-25
 - First Version
