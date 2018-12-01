# nRF52_delay_functions
Included microsecond and millisecond delay functions and an RTC initialization function, all written in FORTH.  These use the nRF52's RTC for timing and are therefore about as accurate as the resolution of the RTC, which is quantized at 31.5 microseconds using prescaler 0 (the most accurate of the prescalers).
