using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FE640
{
    public class ThresholdAccepting : Heuristic
    {
        public ThresholdAccepting(HarvestUnits units)
            : base(units)
        {
        }

        public TimeSpan Accept()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //    T = 200      ' target harvest level for all periods

            //    nrep = 1000  ' number of repetitions at each threshold

            //    For i = 1 To 9

            //        soln(i) = 1 + Int(3 * Rnd) ' setting all parcels to a random harvest period
            //        vol(i) = 10 * i            ' putting in some volumes per parcel that are easy to check later
            //        bestsoln(i) = soln(i)     ' initialize best solution

            //    Next i

            //    Erase h

            //    For i = 1 To 9
            //        h(soln(i)) = h(soln(i)) + vol(i)
            //    Next i


            //    '========================================================
            //    ' find initial value of objective function
            //    '========================================================

            //    objnow = 0


            //    For i = 1 To 3
            //        objnow = objnow + (T - h(i)) ^ 2  ' sum squared deviations from goals
            //    Next i

            //    'set globalbest equal to objnow since it is the best we have yet seen

            //    globalbest = objnow

            //    For n = 1 To 6  ' threshold number

            //        For rep = 1 To nrep

            //            x = 1 + Int(Rnd* 9) ' reach in the bag for a harvest unit between 1 to 9

            //            per = 1 + Int(Rnd* 3) ' reach in the bag for a cut period

            //            oldsoln = soln(x)      ' save old cut period

            //            soln(x) = per          ' temporarily put in new cut period

            //            Erase h                ' set harvest period volume to zero

            //            For i = 1 To 9
            //                h(soln(i)) = h(soln(i)) + vol(i)
            //            Next i

            //            tempobj = 0           ' trial value of objective function

            //            For i = 1 To 3
            //                tempobj = tempobj + (T - h(i)) * (T - h(i))
            //            Next i

            //            If tempobj<k(n) * objnow Then

            //                objnow = tempobj

            //                If objnow<globalbest Then

            //                    globalbest = objnow

            //                    For i = 1 To 9
            //                        bestsoln(i) = soln(i)    ' save best we have ever seen
            //                    Next i

            //                End If

            //            Else

            //                soln(x) = oldsoln ' replace old value if move rejected

            //            End If

            //        Next rep

            //    Next n

            //    '==================================================
            //    ' print out solution
            //    '==================================================

            //    For i = 1 To 9

            //        Debug.Print i, bestsoln(i)

            //Next i

            //    Erase h

            //    For i = 1 To 9
            //        h(bestsoln(i)) = h(bestsoln(i)) + vol(i)
            //    Next i

            //    Debug.Print "first  period harvest"; h(1)
            //  Debug.Print "second period harvest"; h(2)
            //  Debug.Print "third  period harvest"; h(3)


            //End Sub

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}
