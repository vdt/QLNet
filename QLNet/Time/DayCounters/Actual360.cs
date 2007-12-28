/*
 This file is part of QLNet Project http://trac2.assembla.com/QLNet
 
 QLNet is a porting of QuantLib, a free-software/open-source library
 for financial quantitative analysts and developers - http://quantlib.org/
 The license is available online at http://quantlib.org/license.shtml.
 
 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace QLNet
{
   public class Actual360 : DayCounter 
   {
      private new class Impl : DayCounter.Impl 
      {
          public override string name() { return "Actual/360"; }
          public override double yearFraction(DDate d1,DDate d2,DDate Start,DDate End)
          {
             return dayCount(d1,d2)/360.0;
          }
      };

      public Actual360()
         : base(new Actual360.Impl()) {}
   }
}