\ This program defines microsecond and millisecond delay functions:
\ delay_uSec  \ (uSec --  ) Delay the given number of microseconds
\ delay_mSec  \ (mSec --  ) Delay the given number of milliseconds
\
\ Note: Before using delay_uSec or delay_mSec for the first time, the RTC and 
\ low-frequency clock must have been previously initialized by calling the initializeRtc function:
\ initializeRtc  \  (  --  )
\
\ At the bottom is a demo example, tickingSeconds.

hex
\ 40000000 CONSTANT NRF_CLOCK
40000008 CONSTANT NRF_CLOCK__LFCLKSTART
4000000C CONSTANT NRF_CLOCK__LFCLKSTOP
40000104 CONSTANT NRF_CLOCK__EVENTS_LFCLKSTARTED
40000518 CONSTANT NRF_CLOCK__LFCLKSRC \ should default to zero


: stop_LFCLK
  1 NRF_CLOCK__LFCLKSTOP ! ;
  
\ starts the LFCLK, and gurantees that on exit the LFCLK has started
: guaranteedStart_LFCLK
  0 NRF_CLOCK__EVENTS_LFCLKSTARTED ! \ clear semaphore
  1 NRF_CLOCK__LFCLKSTART ! \ start the low frequency clock
  begin
    NRF_CLOCK__EVENTS_LFCLKSTARTED @ \ wait until LFCLK is confirmed as started
  until ;
  
\ note: LFCLK must be stopped before LFCLK source can be set.
: initialize_LFCLK
  stop_LFCLK
  0 NRF_CLOCK__LFCLKSRC !  \ set the internal RC as the LFCLK source
  guaranteedStart_LFCLK ;
  
  
  
\ DESCRIPTION:  Converts a target number of microseconds into clock ticks of the low frequency clock
\ (uSec -- clockTicks) Note uSec is an unsigned 32-bit integer
\ DETAILS: compute using integer math: ((uSec * 1000)/30517) = clockTicks
\ ASSUMPTION: prescaler is 0
\ Therefore, each clock tick is 30.517 microseconds
\ Note: As written, the target number of microseconds cannot exceed 0xFFFFFFFF =  4294967295 microseconds
\ = 4294 seconds = 71 minutes = 88889379 clock ticks
\
decimal
\
: computeTicks 1000  30517  u*/ ; 
   
  
hex
\ 40011000 NRF_RTC  \ RTC1
40011000 constant NRF_RTC__TASKS_START
40011004 constant NRF_RTC__TASKS_STOP
40011008 constant NRF_RTC__TASKS_CLEAR
40011504 constant NRF_RTC__COUNTER


: startRtc 1 NRF_RTC__TASKS_START ! ;
: stopRtc 1 NRF_RTC__TASKS_STOP ! ;
: clearRtc 1 NRF_RTC__TASKS_CLEAR ! ;
: rtcCounter NRF_RTC__COUNTER @ ;

\ Note: Rather non-intuitively, the RTC COUNTER will not clear unless RTC is already actively counting.
\ i.e. The RTC counter cannot be cleared if the RTC is already stopped.
\ PRE-CONDITION ASSUMPTION: RTC is already running.
\ ON EXIT: the RTC is stopped and the RTC counter is guaranteed to be cleared to zero.
\
: guaranteeClearAndStopRtc clearRtc stopRtc begin rtcCounter not until ;

\ DESCRIPTION:  Initializes the RTC by starting the LFCLK and clearing the RTC counter
\ PRE-CONDITIONS: none.
\ ON EXIT: the low frequency clock (LFCLK) will be running and the RTC will be stopped and the 
\          RTC counter will be zero.
\
: initializeRtc initialize_LFCLK startRtc guaranteeClearAndStopRtc ;

\ DESCRIPTION:   delay_uSec  \ (uSec --  ) Delay the given number of microseconds
\ PRE-CONDITION:  RTC is assumed to have been previously initialized but presently stopped with its counter cleared to zero.
\ ON-EXIT: the RTC will be stopped and the RTC counter will be zero.
\
: delay_uSec computeTicks startRtc begin dup rtcCounter <= until drop clearRtc stopRtc ;  

\ DESCRIPTION:   delay_mSec  \ (mSec --  ) Delay the given number of milliseconds
\ PRE-CONDITION:  RTC is assumed to have been previously initialized but presently stopped with its counter cleared to zero.
\
: delay_mSec 1000 * delay_uSec ;  

\ DEMO EXAMPLE:
\
decimal
: tickingSeconds initializeRtc begin ." TICK " 1000 delay_mSec  again ;
