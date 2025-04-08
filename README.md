# Selective Pet Protection

This mod is based off of PetProtection ([thunderstore](https://thunderstore.io/c/valheim/p/zebediah49/Pet_Protection/) [github](https://github.com/zebediah49/PetProtection)). 
Rather than protecting all pets, it only protects pets you choose individually to protect. Pets can either be immortal or stunnable, akin to V+'s immortal and essential options. "Immortal" means, that pets will be completely unfazed by damage. "Stunnable" means that rather than dying, pets become stunned for a custom period of time where they will simply freeze in their spot and recover.
I don't think essential/stunnable actually works (anymore) in PetProtection/V+ so I made sure it works for Selective Pet Protection.

# Commands
To make a pet immortal or stunnable, you simply rename them with the commands **!god** or **!stun** respectively. This will not actually rename the pet, but just add the protection. You can always tell the state of protection by the shield icon after the pets name. By default, gold means immortal, light blue means stunnable. You can change the colors to your liking in the config. Use **!none** to remove protection.

**NEW**: You can now toggle the protection with the displayed hotkey (Shift + T by default), when you hover your cursor on the pet. 

![immortal shield](https://i.imgur.com/efYlWi6.jpeg)

# Configuration
You can change the shield color for immortal and stunnable.
You can set the time a pet will be stunned for.
You can change the hotkey to toggle the protection on hover.

# Compatibility
Partial support for [VikingNPC](https://thunderstore.io/c/valheim/p/RustyMods/VikingNPC/). Only toggling via the hotkey (default is Left Shift + T) works. The hotkey tooltip will not show up when you hover over a tamed Viking, but it will still work, apparent by the shield icon. When your Viking is stunned it will also not say "Recovering" when you hover, but it is indeed recovering in the background.

# Known Issues

 - When a pet is stunned and there is no other valid target around, the attacker will keep attacking the stunned pet. This has no effects on the pet, but rather than the attacker roaming around, he will be fixated on the pet. This might be somewhat of a fringe case, since usually a player will be around.
 - When stunned, pets might be locked in their current animation und they recovered
 - Non-physical damage can unstun pets.

# Contact
magnus.jpg on the Valheim Modding Discord

# Changelog
0.2.2 Fixed a Prefix NRE when a Tameable didn't have a Character component (e.g. HardRock)</br>
0.2.1 Partial support for [VikingNPC](https://thunderstore.io/c/valheim/p/RustyMods/VikingNPC/).</br>
0.2.0 Added a hotkey to toggle the protection on hover</br>
0.1.1 Forgot to mention the !none command in the readme