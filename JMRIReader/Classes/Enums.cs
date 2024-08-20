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

        public enum TransitType
        {
            Scripted,
            Generated
        }

        public enum AutomatedTrainRunningStatus
        {
            Starting = 1,
            Waiting = 2,
            PauseBeforeResume = 3,
            Resuming = 4,
            Running = 5,
            Complete = 6,
            Cancelled = 7
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
            Queued,
            Active,
            Traversed
        }
    }
}
