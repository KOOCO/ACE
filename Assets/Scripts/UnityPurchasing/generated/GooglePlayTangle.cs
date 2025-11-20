// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("vC50LfY5wj3nsxA+iltzeMPAoyRV52RHVWhjbE/jLeOSaGRkZGBlZmKQ14TsvwWRMFwbvuo15PVXK7qcAYjSKi88/XVcWoxLiD7BT8GorMxRz4d+JbwPauLRK1LFIuukX7VxbOxNrYs8j4FdKRbcIXoWXOazKecAvp9uuLE6B3Vz2HHfBaM7nZMefaSOAotbMXPfHusBenzNiqeVzebYLedkamVV52RvZ+dkZGXQWENmcUv/XJTfbNQyrZC55udu28BNyu/4NshS9QF8QtP4SHcOKY68EY/Sx8HknaSaesJxjhc1qcWiPjrbaJnlSEhPTqZd9z2TF5LRX4kwsglptfudaFQZnY+1fIwXU2H2L8WpbM8NkXVp99dURe0DpXAU+mdmZGVk");
        private static int[] order = new int[] { 12,12,8,8,5,11,8,7,12,9,13,12,12,13,14 };
        private static int key = 101;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
