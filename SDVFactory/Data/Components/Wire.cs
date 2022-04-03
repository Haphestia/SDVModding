using System;
using System.Text.Json.Serialization;

namespace SDVFactory.Data.Components
{
    public class Wire
    {
        public long InputMachineNumber = -1;
        public long OutputMachineNumber = -1;

        [JsonIgnore]
        public MachineState InputMachine => FGame.Verse.MachineStates[InputMachineNumber];
        [JsonIgnore]
        public MachineState OutputMachine => FGame.Verse.MachineStates[OutputMachineNumber];

        public Wire(MachineState input, MachineState output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (input == output) throw new ArgumentException("Cannot create a wire between a machine and itself!");
            if (input.MachineNumber == -1 || output.MachineNumber == -1) throw new ArgumentException("Cannot create a wire to machine number -1!");
            InputMachineNumber = input.MachineNumber;
            OutputMachineNumber = output.MachineNumber;
        }

        public Wire(long input, long output)
        {
            if (!FGame.Verse.MachineStates.ContainsKey(input)) throw new ArgumentNullException(nameof(input));
            if (!FGame.Verse.MachineStates.ContainsKey(output)) throw new ArgumentNullException(nameof(output));
            if (input == output) throw new ArgumentException("Cannot create a wire between a machine and itself!");
            if (input == -1 || output == -1) throw new ArgumentException("Cannot create a wire to machine number -1!");
            InputMachineNumber = input;
            OutputMachineNumber = output;
        }

        public bool InvolvesMachine(MachineState m)
        {
            if (m.MachineNumber == InputMachineNumber) return true;
            if (m.MachineNumber == OutputMachineNumber) return true;
            return false;
        }

        public bool InvolvesMachineNumber(long n)
        {
            if (n == InputMachineNumber) return true;
            if (n == OutputMachineNumber) return true;
            return false;
        }

        public bool IsInputMachine(MachineState m)
        {
            return m.MachineNumber == InputMachineNumber;
        }

        public bool IsOutputMachine(MachineState m)
        {
            return m.MachineNumber == OutputMachineNumber;
        }

        public MachineState GetOtherMachine(MachineState m)
        {
            if (m.MachineNumber == InputMachineNumber) return FGame.Verse.MachineStates[OutputMachineNumber];
            if (m.MachineNumber == OutputMachineNumber) return FGame.Verse.MachineStates[InputMachineNumber];
            return null;
        }
    }
}
