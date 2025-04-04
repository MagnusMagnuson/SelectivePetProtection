# Selective Pet Protection

This mod is based off of PetProtection ([thunderstore](https://thunderstore.io/c/valheim/p/zebediah49/Pet_Protection/) [github](https://github.com/zebediah49/PetProtection)). 
Rather than protecting all pets, it only protects pets you choose individually to protect. Pets can either be immortal or stunnable, akin to V+'s immortal and essential options. "Immortal" means, that pets will be completely unfazed by damage. "Stunnable" means that rather than dying, pets become stunned for a custom period of time where they will simply freeze in their spot and recover.
I don't think essential/stunnable actually works (anymore) in PetProtection/V+ so I made sure it works for Selective Pet Protection.

# Commands
To make a pet immortal or stunnable, you simply rename them with the commands **!god** or **!stun** respectively. This will not actually rename the pet, but just add the protection. You can always tell the state of protection by the shield icon after the pets name. By default, gold means immortal, light blue means stunnable. You can change the colors to your liking in the config. Use **!none** to remove protection.

NEW: You can now toggle the protection with the displayed hotkey (Shift + T by default), when you hover your cursor on the pet. 

![immortal shield](https://i.imgur.com/efYlWi6.jpeg)

# Configuration
You can change the shield color for !god and !stun.
You can set the time a pet will be stunned for.
You can change the hotkey to toggle the protection on hover.

# Known Issues

 - When a pet is stunned and there is no other valid target around, the attacker will keep attacking the stunned pet. This has no effects on the pet, but rather than the attacker roaming around, he will be fixated on the pet. This might be somewhat of a fringe case, since usually a player will be around.
 - When stunned, pets might be locked in their current animation und they recovered

# Contact
magnus.jpg on the Valheim Modding Discord

# Changelog
0.2.0 Added a hotkey to toggle the protection on hover
0.1.1 Forgot to mention the !none command in the readme