namespace JMRIReader.Classes
{
    public class Enums
    {
        public enum direction
        {
            North = 16,
            South = 32,
            East = 64,
            Northeast = 80,
            Southeast = 96,
            West = 128,
            Northwest = 144,
            Southwest = 160,
            CW = 256,
            CCW = 512,
            Left = 1024,
            Right = 2048,
            Up = 4096,
            Down = 8192,
            Notknown = 99
        }

        public enum SignalAspect
        {
            Stop = 4,
            Danger = 3,
            Caution = 2,
            Proceed = 1
        }

        public enum AllocationStatus
        {
            NotAllocated = 0,
            NotAvailable = 1,
            Allocated = 2,
            LostAllocation = 3,
            Occupied = 4
        }

        public enum TransitType
        {
            Scripted,
            Generated,
            TriggeredFromUserTransit,
            TriggeredFromShuttleTrausit,
            ManualRepeating,
            //Triggered,
            UserSelected,
            YardShuffle,
            StationAutomation
        }

        public enum AutomatedTrainRunningStatus
        {
            Scheduled = 0,
            Starting = 1,
            Waiting = 2,
            PauseBeforeResume = 3,
            Resuming = 4,
            Running = 5,
            Complete = 6,
            Cancelled = 7,
            ReadyToDelete = 8
        }

        public enum AutomatedTrainRunningSpeed
        {
            Full = 5,
            Caution = 4,
            Crawl = 3,
            Stop = 2,
            EmergencyStop = 1
        }

        public enum TrainDirection
        {
            Forward = 1,
            Reverse = 0
        }

        public enum JourneySequenceState
        {
            Queued = 0,
            Active = 1,
            EnteredNextBlock = 2,
            Traversed = 3
        }

        public enum transitsectionwhen
        {
             SELECTWHEN = 0,
             ENTRY = 1,   // On entry to Section
             EXIT = 2 , // On exit from Section
             BLOCKENTRY = 3, // On entry to specified Block in the Section
             BLOCKEXIT = 4, // On exit from specified Block in the Section
             TRAINSTOP = 5,  // When train stops
             TRAINSTART = 6, // When train starts 
             SENSORACTIVE = 7, // When specified Sensor changes to Active
             SENSORINACTIVE = 8, // When specified Sensor changtes to Inactive
             PRESTARTDELAY = 9, // delays the throttle going from 0
             PRESTARTACTION = 10 // Actions timed of prestartdelay
        }

        public enum transitsectionwhat
        {
             SELECTWHAT = 0,
             PAUSE = 1,    // pause for the number of fast minutes in mDataWhat (e.g. station stop)
             SETMAXSPEED = 2, // set maximum train speed to value entered
             SETCURRENTSPEED = 3, // set current speed to target speed immediately - no ramping
             RAMPTRAINSPEED = 4, // set current speed to target with ramping
             TOMANUALMODE = 5, // drop out of automated mode, and allow manual throttle control
             SETLIGHT = 6, // set light on or off
             STARTBELL = 7,  // start bell (only works with sound decoder, function 1 ON)
             STOPBELL = 8,   // stop bell (only works with sound decoder, function 1 OFF)
             SOUNDHORN = 9,  // sound horn for specified number of milliseconds 
                                                            // (only works with sound decoder, function 2)
             SOUNDHORNPATTERN = 10, // sound horn according to specified pattern
                                                                   // (only works with sound decoder, function 2)
             LOCOFUNCTION = 11,  // execute the specified decoder function
             SETSENSORACTIVE = 12, // set specified sensor active (offers access to Logix)
             SETSENSORINACTIVE = 13, // set specified sensor inactive
             HOLDSIGNAL = 14,    // set specified signalhead or signalmast to HELD
             RELEASESIGNAL = 15, // set specified signalhead or signalmast to NOT HELD
             ESTOP = 16,   // set ESTOP
             PRESTARTRESUME = 17, // Resume after prestart
             TERMINATETRAIN = 18, // terminate train
             LOADTRAININFO = 19, // terminate train and run traininfo file
             FORCEALLOCATEPASSSAFESECTION = 20,  // attempt to force allocation to safesection beyond next safe section.
        }

        public enum BlockState
        {
            Occupied = 2,
            Unoccupied = 4
        }

        public enum TurnoutState
        {
            Closed = 2,
            Thrown = 4
        }
    }
}
