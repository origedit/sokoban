vocabulary sokoban
also sokoban
sokoban definitions

variable level

0 value file
create directory   128 allot

0 value width
0 value height
0 value size
1024 constant max-size
create field   max-size allot
create screen   max-size allot

variable px
variable py

10 constant #max-goals
variable #boxes
variable #goals

create boxes   #max-goals 2* cells allot

: box   2* cells boxes + ;

: box@   box 2@ swap ;

create goals   #max-goals 2* cells allot

: goal   2* cells goals + ;

: goal@   goal 2@ swap ;

bl constant tblank
char # constant twall
char O constant tplayer
char B constant tbox
char X constant tgoal

: directory! ( str len -- )
 dup 127 > if
  s" the levels directory's name is too long."
  exception throw
 then
 directory over 1+ over c!
 1+ swap cmove
 [char] / directory count + 1- c! ;

: open-file
 directory count
 level @ 0 <<# #s #> s+ #>>
 2dup r/o open-file throw to file
 drop free throw ;

: rest ( field-ptr -- space )
 [ field max-size + ] literal swap - ;

: open
 open-file
 field
 begin
  dup dup rest dup 0= throw
  file read-line throw
 while
  ?dup if dup to width + then
 repeat
 drop
 field - width / to height
 width height * to size
 file close-file throw ;

: coords ( offset - x y )
 width /mod ;

: identified ( offset -- x y )
 dup field + tblank swap c!
 coords ;

: identify
 size 0 do
  field i + c@ case
   tplayer of
    i identified py ! px !
   endof
   tgoal of
    i identified swap #goals @ goal 2!
    1 #goals +!
   endof
   tbox of
    i identified swap #boxes @ box 2!
    1 #boxes +!
   endof
  endcase
 loop ;

: load
 0 #boxes !
 0 #goals !
 open identify ;

: tile-at ( x y -- f )
 width * + field + c@ ;

: box-at ( x y -- f )
 swap
 #boxes @ 0 ?do
  2dup i box 2@ d= if
   2drop i unloop exit
  then
 loop
 2drop -1 ;

: free? ( x y -- f )
 2dup tile-at tblank <> if
  2drop false exit
 then
 box-at -1 <> if
  false exit
 then
 true ;

: go ( x y -- )
 py ! px ! ;

: pair+ ( a b c d -- a+c b+d )
 rot + >r + r> ;

: move ( dx dy -- )
 2dup px @ py @ pair+ 
 2dup free? if go 2drop else
  2dup box-at dup -1 = if
   drop 2drop 2drop
  else
   >r
   2tuck pair+ 2dup free? if
    swap r> box 2!
    go
   else
    rdrop 2drop 2drop
   then
  then
 then ;

: arrow-key ( key -- )
 case
  k-right of 1 0 move endof
  k-up of 0 -1 move endof
  k-left of -1 0 move endof
  k-down of 0 1 move endof
 endcase ;

: draw-item ( tile x y -- )
 width * + screen + c! ;

: draw-player
 tplayer px @ py @ draw-item ;

: draw-boxes
 #boxes @ 0 ?do
  tbox i box@ draw-item
 loop ;

: draw-goals
 #goals @ 0 ?do
  tgoal i goal@ draw-item
 loop ;

: draw-field
 field screen size cmove ;

: draw
 page
 ." level " level @ . cr
 draw-field
 draw-goals
 draw-player
 draw-boxes
 screen dup size + swap ?do
  i width type cr
 width +loop ;

: win? ( -- f )
 #goals @ 0 ?do
  i goal 2@ swap box-at -1 = if
   unloop false exit
  then
 loop
 true ;

: control
 ekey ekey>char if
  case
   [char] r of load endof
   #esc of true rdrop exit endof
  endcase
 else ekey>fkey if
  arrow-key
 else drop then then ;

: win
 1 level +!
 ." congratulations." cr
 ." press any key to continue." cr
 ekey drop
 false rdrop exit ;

: play ( -- stop? )
 begin
  control
  draw
  win? if win then
 again ;

: complete
 page
 ." game complete" cr
 ekey drop ;

: game ( str len level -- )
 level !
 directory!
 begin
  ['] load catch if complete exit then
  draw cr
  ." move using arrow keys." cr
  ." press R when stuck." cr
  ." press Escape to exit." cr
 play until ;

