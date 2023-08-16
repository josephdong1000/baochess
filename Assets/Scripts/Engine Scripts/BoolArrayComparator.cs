using System.Collections;
using System.Collections.Generic;

public class BoolArrayComparator : IEqualityComparer<bool[]> {
    private static BoolArrayComparator _default;

    public static BoolArrayComparator Default {
        get {
            if (_default == null) {
                _default = new BoolArrayComparator();
            }

            return _default;
        }
    }
    
    public bool Equals(bool[] x, bool[] y) {
        return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
    }

    public int GetHashCode(bool[] obj) {
        return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}