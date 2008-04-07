﻿/*
 Copyright (C) 2008 Siarhei Novik (snovik@gmail.com)
  
 This file is part of QLNet Project http://www.qlnet.org

 QLNet is free software: you can redistribute it and/or modify it
 under the terms of the QLNet license.  You should have received a
 copy of the license along with this program; if not, license is  
 available online at <http://trac2.assembla.com/QLNet/wiki/License>.
  
 QLNet is a based on QuantLib, a free-software/open-source library
 for financial quantitative analysts and developers - http://quantlib.org/
 The QuantLib license is available online at http://quantlib.org/license.shtml.
 
 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QLNet;

namespace TestSuite {
    [TestClass()]
    public class InterpolatedYieldCurveTest {
        public class CommonVars {
            #region Values
            public struct Datum {
                public int n;
                public TimeUnit units;
                public double rate;
            };
            public struct BondDatum {
                public int n;
                public TimeUnit units;
                public int length;
                public Frequency frequency;
                public double coupon;
                public double price;
            };

            public Datum[] depositData = new Datum[] {
                new Datum { n = 1, units = TimeUnit.Weeks,  rate = 4.559 },
                new Datum { n = 1, units = TimeUnit.Months, rate = 4.581 },
                new Datum { n = 2, units = TimeUnit.Months, rate = 4.573 },
                new Datum { n = 3, units = TimeUnit.Months, rate = 4.557 },
                new Datum { n = 6, units = TimeUnit.Months, rate = 4.496 },
                new Datum { n = 9, units = TimeUnit.Months, rate = 4.490 }
            };

            public Datum[] fraData = new Datum[] {
                new Datum { n = 1, units = TimeUnit.Months, rate = 4.581 },
                new Datum { n = 2, units = TimeUnit.Months, rate = 4.573 },
                new Datum { n = 3, units = TimeUnit.Months, rate = 4.557 },
                new Datum { n = 6, units = TimeUnit.Months, rate = 4.496 },
                new Datum { n = 9, units = TimeUnit.Months, rate = 4.490 }
            };

            public Datum[] swapData = new Datum[] {
                new Datum { n =  1, units = TimeUnit.Years, rate = 4.54 },
                new Datum { n =  2, units = TimeUnit.Years, rate = 4.63 },
                new Datum { n =  3, units = TimeUnit.Years, rate = 4.75 },
                new Datum { n =  4, units = TimeUnit.Years, rate = 4.86 },
                new Datum { n =  5, units = TimeUnit.Years, rate = 4.99 },
                new Datum { n =  6, units = TimeUnit.Years, rate = 5.11 },
                new Datum { n =  7, units = TimeUnit.Years, rate = 5.23 },
                new Datum { n =  8, units = TimeUnit.Years, rate = 5.33 },
                new Datum { n =  9, units = TimeUnit.Years, rate = 5.41 },
                new Datum { n = 10, units = TimeUnit.Years, rate = 5.47 },
                new Datum { n = 12, units = TimeUnit.Years, rate = 5.60 },
                new Datum { n = 15, units = TimeUnit.Years, rate = 5.75 },
                new Datum { n = 20, units = TimeUnit.Years, rate = 5.89 },
                new Datum { n = 25, units = TimeUnit.Years, rate = 5.95 },
                new Datum { n = 30, units = TimeUnit.Years, rate = 5.96 }
            };

            public BondDatum[] bondData = new BondDatum[] {
                new BondDatum { n =  6, units = TimeUnit.Months, length = 5,  frequency = Frequency.Semiannual, coupon = 4.75, price = 101.320 },
                new BondDatum { n =  1, units = TimeUnit.Years,  length = 3,  frequency = Frequency.Semiannual, coupon = 2.75, price = 100.590 },
                new BondDatum { n =  2, units = TimeUnit.Years,  length = 5,  frequency = Frequency.Semiannual, coupon = 5.00, price = 105.650 },
                new BondDatum { n =  5, units = TimeUnit.Years,  length = 11, frequency = Frequency.Semiannual, coupon = 5.50, price = 113.610 },
                new BondDatum { n = 10, units = TimeUnit.Years,  length = 11, frequency = Frequency.Semiannual, coupon = 3.75, price = 104.070 }
            };

            public Datum[] bmaData = new Datum[] {
                new Datum { n =  1, units = TimeUnit.Years, rate = 67.56 },
                new Datum { n =  2, units = TimeUnit.Years, rate = 68.00 },
                new Datum { n =  3, units = TimeUnit.Years, rate = 68.25 },
                new Datum { n =  4, units = TimeUnit.Years, rate = 68.50 },
                new Datum { n =  5, units = TimeUnit.Years, rate = 68.81 },
                new Datum { n =  7, units = TimeUnit.Years, rate = 69.50 },
                new Datum { n = 10, units = TimeUnit.Years, rate = 70.44 },
                new Datum { n = 15, units = TimeUnit.Years, rate = 71.69 },
                new Datum { n = 20, units = TimeUnit.Years, rate = 72.69 },
                new Datum { n = 30, units = TimeUnit.Years, rate = 73.81 }
            };
            #endregion

            // global variables
            public Calendar calendar;
            public int settlementDays;
            public Date today, settlement;
            public BusinessDayConvention fixedLegConvention;
            public Frequency fixedLegFrequency;
            public DayCounter fixedLegDayCounter;
            public int bondSettlementDays;
            public DayCounter bondDayCounter;
            public BusinessDayConvention bondConvention;
            public double bondRedemption;
            public Frequency bmaFrequency;
            public BusinessDayConvention bmaConvention;
            public DayCounter bmaDayCounter;

            public int deposits, fras, swaps, bonds, bmas;
            public List<Quote> rates, fraRates, prices, fractions;
            public List<BootstrapHelper<YieldTermStructure>> instruments, fraHelpers, bondHelpers, bmaHelpers;
            public List<Schedule> schedules;
            public YieldTermStructure termStructure;

            // cleanup
            //SavedSettings backup;
            //IndexHistoryCleaner cleaner;

            // setup
            public CommonVars() {
                // data
                calendar = new TARGET();
                settlementDays = 2;
                today = calendar.adjust(Date.Today);
                Settings.setEvaluationDate(today);
                settlement = calendar.advance(today,settlementDays, TimeUnit.Days);
                fixedLegConvention = BusinessDayConvention.Unadjusted;
                fixedLegFrequency = Frequency.Annual;
                fixedLegDayCounter = new Thirty360(Thirty360.Thirty360Convention.European);
                bondSettlementDays = 3;
                bondDayCounter = new ActualActual();
                bondConvention = BusinessDayConvention.Following;
                bondRedemption = 100.0;
                bmaFrequency = Frequency.Quarterly;
                bmaConvention = BusinessDayConvention.Following;
                bmaDayCounter = new ActualActual();

                deposits = depositData.Length;
                fras = fraData.Length;
                swaps = swapData.Length;
                bonds = bondData.Length;
                bmas = bmaData.Length;

                // market elements
                rates = new List<Quote>(deposits+swaps);
                fraRates = new List<Quote>(fras);
                prices = new List<Quote>(bonds);
                fractions = new List<Quote>(bmas);
                for (int i=0; i<deposits; i++) {
                    rates.Add(new SimpleQuote(depositData[i].rate/100));
                }
                for (int i=0; i<swaps; i++) {
                    rates.Add(new SimpleQuote(swapData[i].rate/100));
                }
                for (int i=0; i<fras; i++) {
                    fraRates.Add(new SimpleQuote(fraData[i].rate/100));
                }
                for (int i=0; i<bonds; i++) {
                    prices.Add(new SimpleQuote(bondData[i].price));
                }
                for (int i=0; i<bmas; i++) {
                    fractions.Add(new SimpleQuote(bmaData[i].rate/100));
                }

                // rate helpers
                instruments = new List<BootstrapHelper<YieldTermStructure>>(deposits + swaps);
                fraHelpers = new List<BootstrapHelper<YieldTermStructure>>(fras);
                bondHelpers = new List<BootstrapHelper<YieldTermStructure>>(bonds);
                schedules = new List<Schedule>(bonds);
                bmaHelpers = new List<BootstrapHelper<YieldTermStructure>>(bmas);

                IborIndex euribor6m = new Euribor6M();
                for (int i=0; i<deposits; i++) {
                    Handle<Quote> r = new Handle<Quote>(rates[i]);
                    instruments.Add(new DepositRateHelper(r, new Period(depositData[i].n, depositData[i].units),
                                      euribor6m.fixingDays(), calendar,
                                      euribor6m.businessDayConvention(),
                                      euribor6m.endOfMonth(),
                                      euribor6m.dayCounter()));
                }
                for (int i = 0; i < swaps; i++) {
                    Handle<Quote> r = new Handle<Quote>(rates[i + deposits]);
                    instruments.Add(new SwapRateHelper(r, new Period(swapData[i].n, swapData[i].units), calendar,
                                    fixedLegFrequency, fixedLegConvention, fixedLegDayCounter, euribor6m));
                }

                Euribor3M euribor3m = new Euribor3M();
                for (int i=0; i<fras; i++) {
                    Handle<Quote> r = new Handle<Quote>(fraRates[i]);
                    fraHelpers.Add(new FraRateHelper(r, fraData[i].n, fraData[i].n + 3,
                                      euribor3m.fixingDays(),
                                      euribor3m.fixingCalendar(),
                                      euribor3m.businessDayConvention(),
                                      euribor3m.endOfMonth(),
                                      euribor3m.dayCounter()));
                }

                for (int i=0; i<bonds; i++) {
                    Handle<Quote> p = new Handle<Quote>(prices[i]);
                    Date maturity = calendar.advance(today, bondData[i].n, bondData[i].units);
                    Date issue =    calendar.advance(maturity, -bondData[i].length, TimeUnit.Years);
                    List<double> coupons = new List<double>() { bondData[i].coupon / 100.0 };
                    schedules.Add(new Schedule(issue, maturity, new Period(bondData[i].frequency), calendar,
                                               bondConvention, bondConvention, DateGeneration.Rule.Backward, false));
                    bondHelpers.Add(new FixedRateBondHelper(p, bondSettlementDays, bondRedemption, schedules[i],
                                                coupons, bondDayCounter, bondConvention, bondRedemption, issue));
                }
            }
        }

        [TestMethod()]
        public void testLinearDiscountConsistency() {
            Console.WriteLine("Testing consistency of piecewise-linear discount curve...");

            CommonVars vars = new CommonVars();

            var curve = new InterpolatedDiscountCurve<Linear,IterativeBootstrap>(vars.settlement, vars.instruments,
                        new Actual360(), new Handle<Quote>(), 1.0e-12, new Linear());

            testCurveConsistency(curve, vars);
            //testBMACurveConsistency(Discount(), Linear(), vars);
        }

        public void testCurveConsistency(InterpolatedDiscountCurve<Linear, IterativeBootstrap> curve, CommonVars vars) {

            vars.termStructure = curve;
            RelinkableHandle<YieldTermStructure> curveHandle = new RelinkableHandle<YieldTermStructure>();
            curveHandle.linkTo(curve);

            // check deposits
            for (int i=0; i<vars.deposits; i++) {
                Euribor index = new Euribor(new Period(vars.depositData[i].n, vars.depositData[i].units), curveHandle);
                double expectedRate = vars.depositData[i].rate / 100,
                       estimatedRate = index.fixing(vars.today);
                if (Math.Abs(expectedRate-estimatedRate) > 1.0e-9) {
                    Assert.Fail(vars.depositData[i].n + " "
                        + (vars.depositData[i].units == TimeUnit.Weeks ? "week(s)" : "month(s)")
                        + " deposit:"
                        + "\n    estimated rate: " + estimatedRate
                        + "\n    expected rate:  " + expectedRate);
                }
            }

            // check swaps
            IborIndex euribor6m = new Euribor6M(curveHandle);
            for (int i=0; i<vars.swaps; i++) {
               Period tenor = new Period(vars.swapData[i].n, vars.swapData[i].units);

                VanillaSwap swap = new MakeVanillaSwap(tenor, euribor6m, 0.0)
                    .withEffectiveDate(vars.settlement)
                    .withFixedLegDayCount(vars.fixedLegDayCounter)
                    .withFixedLegTenor(new Period(vars.fixedLegFrequency))
                    .withFixedLegConvention(vars.fixedLegConvention)
                    .withFixedLegTerminationDateConvention(vars.fixedLegConvention);

                double expectedRate = vars.swapData[i].rate / 100,
                     estimatedRate = swap.fairRate();
                double tolerance = 1.0e-9;
                double error = Math.Abs(expectedRate-estimatedRate);
                if (error > tolerance) {
                   Assert.Fail(vars.swapData[i].n + " year(s) swap:\n"
                        + "\n estimated rate: " + estimatedRate
                        + "\n expected rate:  " + expectedRate
                        + "\n error:          " + error
                        + "\n tolerance:      " + tolerance);
                }
            }

            // check bonds
            vars.termStructure = new InterpolatedDiscountCurve<Linear, IterativeBootstrap>(vars.settlement, vars.bondHelpers,
                                     new Actual360(), new Handle<Quote>(), 1.0e-12, new Linear());
            curveHandle.linkTo(vars.termStructure);

            for (int i=0; i<vars.bonds; i++) {
                Date maturity = vars.calendar.advance(vars.today,
                                                      vars.bondData[i].n,
                                                      vars.bondData[i].units);
                Date issue = vars.calendar.advance(maturity,
                                                   -vars.bondData[i].length,
                                                   TimeUnit.Years);
                List<double> coupons = new List<double>() { vars.bondData[i].coupon / 100.0 };

                FixedRateBond bond = new FixedRateBond(vars.bondSettlementDays, 100.0,
                                   vars.schedules[i], coupons,
                                   vars.bondDayCounter, vars.bondConvention,
                                   vars.bondRedemption, issue);

                PricingEngine bondEngine = new DiscountingBondEngine(curveHandle);
                bond.setPricingEngine(bondEngine);

                double expectedPrice = vars.bondData[i].price,
                     estimatedPrice = bond.cleanPrice();
                double tolerance = 1.0e-9;
                if (Math.Abs(expectedPrice-estimatedPrice) > tolerance) {
                   Assert.Fail(i+1 + " bond failure:" +
                                "\n  estimated price: " + estimatedPrice +
                                "\n  expected price:  " + expectedPrice);
                }
           }

           // check FRA
            vars.termStructure = new InterpolatedDiscountCurve<Linear, IterativeBootstrap>(vars.settlement, vars.fraHelpers,
                                        new Actual360(), new Handle<Quote>(), 1.0e-12, new Linear());
            curveHandle.linkTo(vars.termStructure);

            IborIndex euribor3m = new Euribor3M(curveHandle);
            for (int i=0; i<vars.fras; i++) {
                Date start = vars.calendar.advance(vars.settlement,
                                          vars.fraData[i].n,
                                          vars.fraData[i].units,
                                          euribor3m.businessDayConvention(),
                                          euribor3m.endOfMonth());
                Date end = vars.calendar.advance(start, 3, TimeUnit.Months,
                                                 euribor3m.businessDayConvention(),
                                                 euribor3m.endOfMonth());

                ForwardRateAgreement fra = new ForwardRateAgreement(start, end, Position.Type.Long, vars.fraData[i].rate / 100,
                                                                    100.0, euribor3m, curveHandle);
                double expectedRate = vars.fraData[i].rate / 100,
                       estimatedRate = fra.forwardRate().rate();
                double tolerance = 1.0e-9;
                if (Math.Abs(expectedRate-estimatedRate) > tolerance) {
                   Assert.Fail(i+1 + " FRA failure:" +
                                "\n  estimated rate: " + estimatedRate +
                                "\n  expected rate:  " + expectedRate);
                }
            } 
        }

        public void suite() {
            testLinearDiscountConsistency();
        }
    }
}
