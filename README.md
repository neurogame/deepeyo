# deepeyo
Game similar to the popular io game, written completely in VB.NET and GDI+

## Downloading
Download `deepeyo.exe` for a pre-compiled Windows version of Deepeyo client (the actual game) version 1.0.
Download `deepserv.exe` for a pre-compiled Windows version of Deepeyo server (for hosting games) version 1.0.

## Playing
Follow the instructions under **Downloading** for the client. Run it, ignore any security errors (this is due to no certificates), and type the IP address of anyone you know hosting a server.

## Hosting
Follow the instructions under **Downloading** for the server. Run it, ignore any security errors, and then close it again. Look in the directory you started it in, there should be a new file called `server.ini`. Open this, edit any properties (you should use the **Settings Index**) and then run it. 

### For testing
Follow the instructions for **Hosting**, but in the `settings.ini` file, bind your server to the local host, by setting `address` to `127.0.0.1` or your private IPv4. You can then connect to it by using the address `127.0.0.1` **on your own machine**.

## Tips and tricks
Pressing CTRL-R at any time in the console reloads the settings. Pressing CTRL-N at any time will summon a bot.

## Settings Index

#### address
Specifies the address to bind the listening socket to.
#### port
Specifies the port to listen on.
#### timeout
Specifies the maximum amount of time (in milliseconds) between requests before a user will time out. Keep high, above 10 seconds.
#### allow_multi_connect
Specifies whether the same IP can be connected via multiple Deepeyo instances.
#### world_size
Sets the size in pixels of the world, centered around (0, 0). Keep big for larger servers, smaller for private servers.
#### base_size
Sets the size in pixels of the team bases. Keep small, make sure this value multiplied by 2 does not come close to or exceed `world_size`.
#### maximum_party
Specifies the maximum amount of connected players (excluding bots). *Not yet implemented*
#### max_size
Specifies the maximum size in pixels of a player.
#### max_life
Specifies the starting health, and maximum health of a player.
#### max_vel
Specifies the maximum velocity of a player.
#### size_addition
Sets an amount of pixels to add to every player's size.
#### size_score_division
The size of a player will be set to the player's score divided by this value.
#### acceleration
Velocity of a player in both axes will be increased by this amount each time the player moves along an axis.
#### dissipation
Velocity of a player is slowly dissipated. The formula of the dissipation is `(v -= v / d)`, while `d` is the value for this property, and `v` is the current velocity.
#### collision_division
On a player's collision with another player, the player's velocity is increased by the velocity of this player divided by the value for this property.
#### damage
A player's health points are decreased by this amount on collision with a bullet.
#### knockback_division
On a bullet's collision with a player, the player is pushed in the direction of this bullet, the force is divided by this value.
#### bullet_deflect
When a bullet hits a team base of the opposing team, the bullet's velocity is decreased/increases (depending on the side) by this value. Should be positive.
#### shoot_interval
If the player is shooting, and the tick is divisible by this value then a bullet will spawn.
#### bullet_speed
Any new bullets have a velocity of this value.
#### kill_multiplier
When a player kills another player, the score received by the killing player is equal to the victim player's score multiplied by this value.
#### starting_score
Every player begins with an initial score of this value. If value is 0, no score can be gained by any players.
