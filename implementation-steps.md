## Player melee attack
- Create Layer for enemy (if not created) and assign it
- create 4 variables
  - attack range (float)
  - attack damage (int)
  - attack layers (LayerMask)
  - attack point (Transform)
- create input type 'Attack'
- listen to input and apply attack (see code in git)
- create attack point transfor to player
- create  gizmo
- assign variables and play test
- TODO - animation

## Player melee animation
- new animation - have it short, with some anticipation and longer end stance
- transition to it from any state, but do not have transition durations for any of them
- create new trigger "Attack" (constants and animator)