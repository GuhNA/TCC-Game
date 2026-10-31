# Prompts — edição com ferramenta integrada

## smoothUpPrompt

Edit this hero's rear-view walk cycle into a smooth, anatomically coherent 12-frame animation sprite sheet. Input image is identity, pixel-art style and six key walking poses reference, NOT an instruction to mirror the whole body. Preserve character proportions, brown hair gold headband, dark wine-red cape gold border, green tunic, cream sleeves brown bracers, bluegrey trousers, brown boots and dark grouped pixel shading. Transparent RGBA background.
Output EXACTLY 6 columns x 2 rows of equally spaced full-body sprites, chronological row-major, 12 unique consecutive poses in one loop. Aim canvas1956x602, cells326x301, fixed root at local(163,285), full character about260px tall, no cropping, no grid/text/shadows. Head and torso anchored same central axis with only 3px natural vertical bob, feet stay in ground reference.
CRITICAL WEAPON: one sheathed sword ONLY on character's anatomical LEFT hip, SCREEN LEFT in this rear view, in ALL TWELVE FRAMES. Brown scabbard with gold tip beside left thigh, never swap sides. His LEFT elbow slightly bent and LEFT hand resting firmly on the sword pommel at left waist in every frame. Hand follows hip gently, NEVER swings away, never draws sword. RIGHT arm only is free and swings opposite RIGHT leg naturally. No weapon on screen right, do not mirror the upper body, sword or hands.
Legs follow the input's alternating two-step idea but add actual in-between poses: row1 starts left leg extended backward toward viewer (left boot lower on screen, right boot lifted farther away), gradually left leg supports and right leg swings through; row2 swaps leg phases ONLY (right boot backward toward viewer, left leg swings). Poses1 contact left supporting/right trailing,2 weight down,3 toe-off right with knee bent,4 passing right foot alongside left,5 right foot advances away/up-screen,6 approach opposite contact. Poses7–12 repeat anatomical phases with legs exchanged, NOT mirrored equipment. Make large enough distinct boot changes to clearly read as walking AWAY from viewer, not hopping or both legs extending together; one foot always supports.
Cape attached fixed at shoulders, modest hem wave delayed one frame behind hips, consistent length/goldborder, not a stiff repeated shape nor wildly expanding. Maintain reference art detail/palette; no blur or interpolation ghosting, crisp pixel outlines. Seamless frame12-to-frame1 transition, no duplicate held frames. Frame7 MUST have opposite leading leg from frame1 while sword+restinghand stay screen LEFT. Prioritize these requirements over decorative detail.

## smoothUpCorrectionPrompt

TARGETED ANIMATION CORRECTION. Edit this 6x2 twelve-frame rear-view hero sprite sheet. Preserve pixel art, identity, colors, costume, upper torso, cape, SINGLE LEFT-side sheathed sword and LEFT hand resting on pommel in every cell. Transparent background. The current image is WRONG because ten frames have the same left boot extended. Correct ONLY the leg cycle and right free arm in all cells to create TWO alternating steps, not the same step twice. Do not merely duplicate the source poses.
Exact leg chart for each of the 12 cells, read left-to-right top row then bottom:
1 left boot LOW toward viewer and planted; right boot HIGH away tucked under cape.
2 left boot moves slightly UP toward body; right toe pushes off and knee bends.
3 both boots almost LEVEL in Y; RIGHT boot lifted at ankle, knees pass. Narrow stride, not wide.
4 RIGHT boot now LOW toward viewer; LEFT boot HIGH away under cape. This is OPPOSITE to frame1.
5 right boot stays low supporting, left toe begins lifting.
6 boots approaching LEVEL; LEFT knee bends and comes through.
7 LEFT boot LOW and right HIGH, intermediate longer stride.
8 left boot moves up; right foot swings downward out of cape.
9 both boots LEVEL at passing; RIGHT heel off ground.
10 RIGHT boot LOW and left HIGH.
11 right boot moves up supporting; LEFT foot swings down out of cape.
12 both boots LEVEL with LEFT foot about to plant in frame1.
IMPORTANT four passing poses 3,6,9,12 MUST have feet nearly side-by-side at same screen height and narrower stance, not one long extended leg. Four opposite contacts are clear. Keep same boot design. Free RIGHT arm moves forward/back in opposite phase to RIGHT leg, left arm always hand on pommel at screen left waist. Never mirror sword to other side. Keep consistent character size, base registration, cell spacing, slight bob only. This must read as continuous walking when cycled. No text, no grids, no shadow, alpha transparent.

## passingPrompt

Create ONLY FOUR full-body sprites of THIS SAME rear-view hero, in a single horizontal row of FOUR equal cells, transparent background, no labels. This is a focused in-between animation study, not another twelve-frame sheet.
Preserve the source's exact costume, hair, wine-red cape with gold hem, proportions, pixel shading. ONE sheathed sword on SCREEN LEFT hip in ALL FOUR sprites, left hand resting on pommel, elbow bent. Right arm free. Both legs are visible below cape. Rear view, facing directly away.
The four poses MUST BE CLEARLY DIFFERENT from the source's long stride. 
Sprite1 PASSING pose: Both boots are NEXT TO EACH OTHER at approximately the SAME HEIGHT, feet under hips, left leg straight supporting, right knee bent and right heel slightly lifted. Narrow vertical stance, NO long boot sticking toward viewer. Right free arm passes torso.
Sprite2 OPPOSITE CONTACT pose: RIGHT boot extended LOW toward bottom of canvas and planted, LEFT boot lifted HIGH under cape. Sword still SCREEN LEFT, NOT flipped with legs.
Sprite3 OPPOSITE PASSING pose: Both boots NEXT TO EACH OTHER at approximately SAME HEIGHT under hips; RIGHT leg straight supporting and LEFT knee bent, left heel slightly raised. Right free arm passes torso.
Sprite4 LEFT CONTACT pose: LEFT boot extended LOW toward bottom of canvas and planted, RIGHT boot lifted HIGH under cape.
Exact same head and hip center in all cells, same character scale and ground line. Small cape flutter only. DO NOT reproduce the left-long-leg pose four times. Sprites1 and3 must visibly show close, level feet as opposed to extended strides2 and4. Keep sword and hand anatomically unchanged side. Render as clean game-ready pixel sprites matching reference, not painterly, no anti-aliased glow. True transparent alpha. Four columns one row.
