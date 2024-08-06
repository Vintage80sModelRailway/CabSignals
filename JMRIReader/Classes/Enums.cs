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
            Danger,
            Caution,
            Proceed,
            Stop
        }

        public enum AutomatedTrainRunningStatus
        {
            Starting,
            RampingUp,
            Running,
            RampingDown,
            Caution,
            Waiting,
            Complete
        }

        public enum AutomatedTrainRunningSpeed
        {
            Full,
            Caution,
            Crawl,
            Stop
        }

        public enum TrainDirection
        {
            Forward = 1,
            Reverse = 0
        }
    }
}
