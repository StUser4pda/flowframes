﻿using Flowframes.IO;
using Flowframes.MiscUtils;
using System.IO;
using System.Linq;

namespace Flowframes.Data
{
    public class AiInfo
    {
        public enum AiBackend { Pytorch, Ncnn, Tensorflow, Other }
        public AiBackend Backend { get; set; } = AiBackend.Pytorch;
        public string NameInternal { get; set; } = "";
        public string NameShort { get { return NameInternal.Split(' ')[0].Split('_')[0]; } }
        public string NameLong { get; set; } = "";
        public string FriendlyName { get { return $"{NameShort} ({GetFrameworkString(Backend)})"; } }
        public string Description { get { return $"{GetImplemString(Backend)} of {NameShort}{(Backend == AiBackend.Pytorch ? " (Nvidia Only!)" : "")}"; } }
        public string PkgDir { get { return NameInternal.Replace("_", "-").Lower(); } }
        public enum InterpFactorSupport { Fixed, AnyPowerOfTwo, AnyInteger, AnyFloat }
        public InterpFactorSupport FactorSupport { get; set; } = InterpFactorSupport.Fixed;
        public int[] SupportedFactors { get; set; } = new int[0];
        public bool Piped { get; set; } = false;

        public string LogFilename { get { return PkgDir + "-log"; } }

        public AiInfo() { }

        public AiInfo(AiBackend backend, string aiName, string longName, InterpFactorSupport factorSupport = InterpFactorSupport.Fixed, int[] supportedFactors = null)
        {
            Backend = backend;
            NameInternal = aiName;
            NameLong = longName;
            SupportedFactors = supportedFactors;
            FactorSupport = factorSupport;
        }

        public string GetVerboseInfo()
        {
            return $"Name:\n{NameShort}\n\n" +
                $"Full Name:\n{NameLong}\n\n" +
                $"Inference Framework:\n{FormatUtils.CapsIfShort(Backend.ToString(), 5)}\n\n" +
                $"Hardware Acceleration:\n{GetHwAccelString(Backend)}\n\n" +
                $"Supported Interpolation Factors:\n{GetFactorsString(FactorSupport)}\n\n" +
                $"Requires Frame Extraction:\n{(Piped ? "No" : "Yes")}\n\n" +
                $"Package Directory/Size:\n{PkgDir} ({FormatUtils.Bytes(IoUtils.GetDirSize(Path.Combine(Paths.GetPkgPath(), PkgDir), true))})";
        }

        private string GetImplemString(AiBackend backend)
        {
            if (backend == AiBackend.Pytorch)
                return $"CUDA/Pytorch Implementation";

            if (backend == AiBackend.Ncnn)
                return $"Vulkan/NCNN{(Piped ? "/VapourSynth" : "")} Implementation";

            if (backend == AiBackend.Tensorflow)
                return $"Tensorflow Implementation";

            return "";
        }

        private string GetFrameworkString(AiBackend backend)
        {
            if (backend == AiBackend.Pytorch)
                return $"CUDA";

            if (backend == AiBackend.Ncnn)
                return $"NCNN{(Piped ? "/VS" : "")}";

            if (backend == AiBackend.Tensorflow)
                return $"TF";

            return "Custom";
        }

        private string GetHwAccelString(AiBackend backend)
        {
            if (Backend == AiBackend.Pytorch)
                return $"GPU (Nvidia CUDA)";

            if (Backend == AiBackend.Ncnn)
                return $"GPU (Vulkan)";

            return "Unknown";
        }

        private string GetFactorsString(InterpFactorSupport factorSupport)
        {
            if (factorSupport == InterpFactorSupport.Fixed)
                return $"{string.Join(", ", SupportedFactors.Select(x => $"{x}x"))}";

            if (factorSupport == InterpFactorSupport.AnyPowerOfTwo)
                return "Any powers of 2 (2/4/8/16 etc.)";

            if (factorSupport == InterpFactorSupport.AnyInteger)
                return "Any integer (whole number)";

            if (factorSupport == InterpFactorSupport.AnyFloat)
                return "Any, including fractional factors";

            return "Unknown";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (AiInfo)obj;
            return Backend == other.Backend && NameInternal == other.NameInternal;
        }

        // Combine hash codes of properties (using a simple hash approach for .NET Framework)
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Backend.GetHashCode();
            hash = hash * 23 + (NameInternal?.GetHashCode() ?? 0);
            return hash;
        }

        public static bool operator ==(AiInfo left, AiInfo right)
        {
            if (left is null)
                return ReferenceEquals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(AiInfo left, AiInfo right) => !(left == right);
    }
}
