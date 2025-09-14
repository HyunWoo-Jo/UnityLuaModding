using System;
namespace Modding {
    [Serializable]
    public class ModDependency {
        public string name;
        public string version = "1.0.0";
        public string @operator = ">=";

        /// <summary>
        /// string Version to System Version
        /// Utility 함수 문자열 버전 -> 시스템 버전
        /// </summary>
        public System.Version GetVersion() {
            if (System.Version.TryParse(version, out System.Version ver))
                return ver;
            return new System.Version(1, 0, 0);
        }

        /// <summary>
        /// Check version
        /// 버전 조건 확인
        /// </summary>
        public bool CheckVersion(System.Version targetVersion) {
            var requiredVersion = GetVersion();

            switch (@operator) {
                case ">=":
                return targetVersion >= requiredVersion;
                case ">":
                return targetVersion > requiredVersion;
                case "<=":
                return targetVersion <= requiredVersion;
                case "<":
                return targetVersion < requiredVersion;
                case "==":
                case "=":
                return targetVersion == requiredVersion;
                case "!=":
                return targetVersion != requiredVersion;
                case "~>": // Compatible version (same major.minor, patch can be higher)
                return targetVersion.Major == requiredVersion.Major &&
                       targetVersion.Minor == requiredVersion.Minor &&
                       targetVersion >= requiredVersion;
                default:
                return targetVersion >= requiredVersion;
            }
        }

        public override string ToString() {
            return $"{name} {@operator} {version}";
        }
    }
}