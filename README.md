

This is a prototype project built using the Forge Networking Remastered library.
[Official Documentation](http://docs.forgepowered.com/)


This was a first attempt at making a basic FPS style prototype.
This code was put together with the idea of making something to evaluate the library with.
A lot of this I would not do in a production type of setup. 

So as a warning just look at this code do not try to use it directly in a real project.

Modifications to Forge are as follows
Be able to select between the standard NCW fields and a snapshot mode.
In snapshot mode the network object is created to work with snapshot mode. (Uses a new template)

## The snapshot mode 
- Create a struct from the NCW fields
- Sets up new Serialize and Read Dirty fields handlers
- Adds a Linked list of snapshots
- Adds functions to work with this snapshot array
- *Initial support of rewinding (WIP)
- Interpolation now happens between two snapshots based on the tick
- 1 Tick = 16ms, 16, 32, 48 etc. (local render time should be in between the steps to get good interpolation) TODO
- Sim is running at 60hz. At this speed interpolation is not that useful.

The prototype assumes that a single player will be the server and client. Other players connect to this server.
The basic control and network object is the PlayerCharacter, internally it handles all the various setups.
Server, Remote Player, Server w/Local Player 

Everything is executed in FixedUpdate.

## Server Player
Every fixed update the server player character will proces an input from its remote/local player.
And will send out a snapshot to all clients.
The server maintains a seperate list of all snapshots. While the client looks at 200ms, the server stores 1000ms worth to be used for rewinding.

There will then be a local or remote player for each server Player

## Remote Player
Will receieve this snapshot and add it to their internal buffer.
The will then interpolate their state between these snapshots. The time that the Remote displays is 200ms behind.

## Local Player 
Will collect input in fixed update.
It sends this input to the server and applies it locally on its self to predict what the server will do. 
It stores this input in its local playback buffer.
Inputs are sent to the server unreliably. But with X (3) previous inputs. The server will only apply these inputs if they are newer.
Also the inputs are never acked, so it is possible that inputs can be missed if too many packets fail.
It will ignore the snapshots for the most part. It only uses the snapshot as a validation to its prediction.
When it recieves a snapshot. It resets the player to this location. Based on the snapshots last proccesed command the local player updates his predicted inputs.
It removes any already processed inputs. Then it will replay the remaining inputs on top of the snapshot state.
If the prediction was successful, then there should be no noticable changes to the client.

The GameManager object tries to synchronize time. (Not the best example) 

The GameInfo class trys to manage starting of the demo etc. (Not the best example) 

The PlayerModel class tries to have the players visuals track the server representation to provide some smoothing. (Not the best example) 
But isn't needed so much with the snapshots now. Is leftover from earlier testing.

There are some other initial attemps at some weapons etc.
Most of these use their own RPC's. This system needs to be redone and integerated into the standard inputs system.
As well as making use of the snapshots system and rewind.

I also tried to comment some of the core player code.

If you have questions you can read me on discord at @shadowworm#6629
