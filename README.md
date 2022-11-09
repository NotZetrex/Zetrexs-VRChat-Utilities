# Zetrex's VRChat Utilities
**This itself does not interact with VRChat directly like a mod or anything, it just starts VRChat, that is all.**

I originally created this to get into VRChat faster by starting all my programs related to VRC along with it, then it slowly turned into a general purpose program that I thought others would find useful.

# What it do?
## Launches programs on startup
Have you ever found it annoying to have to start your osc apps one by one when trying to just vibe in VRC? No? Well I did, so I created an "easy" system to configurate programs to start with VRC when pressing play.
![VRCUtil-Program-Launch-Example](https://user-images.githubusercontent.com/102548737/200729103-4fb01c36-5fe1-48d9-aa74-2aaccc2d3c43.gif)

## Easy program reloading from your avatar!
I always had issues where my face tracking would die and I'd need to restart VRCFT to get it working again. You can add a parameter in your avatar with "Restart" then your programs executable name, for example: "RestartVRCFaceTracking".

On a side note, this also restarts programs automaticly if it closes/crashes (Toggleable option in the program config)![VRCUtil-Program-Auto-Restart-Example](https://user-images.githubusercontent.com/102548737/200730879-8dd70e77-8604-4374-8864-77afc2a44132.gif)

## OSC Router
I've tried using other routers on github but they all had the problem that some osc apps ~~VRCFaceTracking~~ would send too many parameters and cause the router to fall behind, this one does not do that. I know, crazy 🥴.

![image](https://user-images.githubusercontent.com/102548737/200731457-5c77b4cb-98ea-4718-9982-0acbe4afe2ec.png)

## Time limit
Now this is more of a me thing I'm assuming but I made it so if you want, your game will not open past a certian time and alt+f4's when you're on when the specified time is reached. Can also set a time in the morning which only after you can launch the game.

Sends these parameters to vrc every second or so if the OSC Router is enabled.

(Int)TimeLeftSec

(Int)TimeLeftMin

(Int)TimeLeftHour

(Float)TimeLeftFloat **NO WORKY**

Also sends chatbox messages with the time remaining at certain increments.

![image](https://user-images.githubusercontent.com/102548737/200731900-3568f18d-54d7-4c71-8167-5bd717c15ede.png)

# Launch Parameters
If you're using steam you can add these to the "Launch Options" in a game.

**_Not_ case sensitive**

## -Debug (Self Explanatory)
Mainly shows extra logs in the console window
![image](https://user-images.githubusercontent.com/102548737/200732589-04668b4e-c3e9-42ef-a036-291313fd6f17.png)

## -Setup
Enables setup mode in where you can configure what programs you want to launch, your timelimit, and osc router addresses.
![image](https://user-images.githubusercontent.com/102548737/200732551-6138a988-cd12-4914-8312-69e58a3e96ca.png)

## -NoLaunch
Stops any programs from the config to launch.

## -NoVRC
Stops VRChat from launching, mainly if you want to mess with something related to the program itself without waiting for vrc to open and close every time.

## -RandIMG (**BROKEN**)
Randomises the EAC backround image from a list if images in a folder. Still working on this.

## -TimeLimit
Enables the time limit.

# Installation

## 1. Rename the EAC executeable
Goto your VRChat directory and locate the "start_protected_game.exe" and rename it to "EAC_Launch.exe".

## 2. Download/Build the utility and place it in your VRC folder
This program uses the old name of the EAC launcher to be ran instead of the game itself, so just download the program and place it in your VRChat folder.

## And BOOM! Done!

# Current Bugs

## Small chance of a random crash when rebooting a program
I made it so you can use a shortcut to launch the utility again while vrchat is running and continue like nothing happened.
