using GenericAPIProtos;
using Google.Protobuf.WellKnownTypes;
using LibRtDb.DTO.Rates;
using LibRtDb.DTO.Rates.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.Logics
{
    public static class RateExt
    {

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        #region Helpers
        /// <summary>
        /// Allows to iterate through every single day
        /// https://stackoverflow.com/questions/1847580/how-do-i-loop-through-a-date-range
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        public static List<PeriodDTO> ChunkInSingleDayPeriods(PeriodDTO Period)
        {
            var finalResult = new List<PeriodDTO>();
            for (var day = Period.From.Date; day.Date <= Period.To.Date; day = day.AddDays(1))
            {

                var result = new PeriodDTO();

                //Starting date
                if (day == Period.From.Date)
                {
                    result.From = Period.From;
                    if (day == Period.To.Date)
                    {
                        //period is in same day
                        result.To = Period.To;
                    }
                    else
                    {
                        //period contains more days
                        result.To = day.AddDays(1);
                    }
                }
                //Ending date
                else if (day == Period.To.Date)
                {
                    //becouse at this point we already iterated to next day
                    result.From = day;
                    result.To = Period.To;
                }
                //All Intermediate cases
                else
                {
                    //becouse at this point we already iterated to next day
                    result.From = day;
                    result.To = day.AddDays(1);
                }

                finalResult.Add(result);

            }

            return finalResult;

        }

        /// <summary>
        /// Selects first Enclosing time Belt and its index in array
        /// </summary>
        /// <param name="CurBelts"></param>
        /// <param name="InitialFromDuration"></param>
        /// <param name="FinalToDuration"></param>
        /// <returns></returns>
        private static (int Index, TimeBelt Item, bool IsCrossDayBelt) GetFirstUsefulTimeBeltIndex(List<TimeBelt> CurBelts, TimeSpan InitialFromDuration, TimeSpan FinalToDuration)
        {
            //First of all lets assume that time belts are ordered
            //Lets find first useful timeBelt

            TimeSpan dayStart = new TimeSpan(0, 0, 0);

            TimeSpan curFrom = new TimeSpan();
            TimeSpan curTo = new TimeSpan();
            for (var i = 0; i < CurBelts.Count; i++)
            {

                curFrom = CurBelts[i].From.ToTimeSpan();
                curTo = CurBelts[i].To.ToTimeSpan();

                bool isCrossDayBelt = curFrom > curTo;

                if (isCrossDayBelt == false)
                {
                    //do as usual, ensure our period is starting in that specific belt
                    if (InitialFromDuration >= curFrom && InitialFromDuration < curTo)
                    {
                        //found what we searched
                        return (i, CurBelts[i], false);
                    }
                }
                else
                {
                    //its a crossday belt
                    if (
                        (InitialFromDuration >= curFrom && FinalToDuration >= curTo)
                        ||
                        (InitialFromDuration >= dayStart && InitialFromDuration < curTo)
                        )
                    {
                        return (i, CurBelts[i], true);
                    }
                }

            }

            log.Error("No Appropriate TimeBelt Found!");

            return (-1, null, false);

        }

        /// <summary>
        /// Gets To (TimeSpan) of CrossDay belt.
        /// In other word gets belt end
        /// </summary>
        /// <param name="OriginalFrom"></param>
        /// <param name="DayModel"></param>
        /// <returns></returns>
        private static TimeSpan GetCrossDayTermination(TimeSpan OriginalFrom, WeekModelDay DayModel){
            var crossDayBelt = DayModel.TimeBelts.Where(x => x.From.ToTimeSpan() > x.To.ToTimeSpan()).FirstOrDefault();
            if(crossDayBelt != null)
            {
                OriginalFrom = crossDayBelt.To.ToTimeSpan();
            }
            return OriginalFrom;
        }

        /// <summary>
        /// Chunks Passed Day in Appropriate belts quantity
        /// </summary>
        /// <param name="DayPeriod"></param>
        /// <param name="DayModel"></param>
        /// <returns></returns>
        public static ChunkDayInBelts_ResultDTO ChunkDayInBelts(PeriodDTO DayPeriod, WeekModelDay DayModel, RateCalculationDTO PrevRateCalc)
        {

            log.Trace("ChunkDayInBelts Invoked!");

            var result = new ChunkDayInBelts_ResultDTO();

            //Its whole Day
            if(DayPeriod.From.AddDays(1) == DayPeriod.To)
            {
                log.Debug("We found 24h day, will just return day belts");
                foreach(var timeBelt in DayModel.TimeBelts)
                {
                    result.DayBelts.Add(new DayBeltChunkDTO()
                    {
                        From = timeBelt.From.ToTimeSpan(),
                        To = timeBelt.To.ToTimeSpan(),
                        TimeBelt = timeBelt
                    });
                }
            }
            else
            {

                TimeSpan initialFromDuration = DayPeriod.From.TimeOfDay;
                if (PrevRateCalc != null && PrevRateCalc.IsCrossDayDetected == true)
                {
                    initialFromDuration = GetCrossDayTermination(DayPeriod.From.TimeOfDay, DayModel);
                }

                var finalToDuration = DayPeriod.To.TimeOfDay.TotalSeconds > 0 ? DayPeriod.To.TimeOfDay : new TimeSpan(24, 0, 0);
                var curBelts = DayModel.TimeBelts.ToList();

                log.Trace($"initialFromDuration: {initialFromDuration}, finalToDuration: {finalToDuration}, curBelts: {curBelts.Count}");

                var firstBelt = GetFirstUsefulTimeBeltIndex(curBelts, initialFromDuration, finalToDuration);

                var tmpFristBeltItemFrom = firstBelt.Item.From.ToTimeSpan();
                var tmpFristBeltItemTo = firstBelt.Item.To.ToTimeSpan();

                var correctFrom = initialFromDuration > tmpFristBeltItemFrom ? initialFromDuration : tmpFristBeltItemFrom;
                var correctTo =  finalToDuration < tmpFristBeltItemTo ? finalToDuration : tmpFristBeltItemTo;

                //Warning This little if is needed in cases of CrossDay
                //In particular on last day of period
                //Thanks to GetCrossDayTermination() initialFromDuration on last day will advance too forward
                //In this way we can understand that we no need pay for this period as it were payed in first day
                //And thanks to propagated calculation that surplus propagated until last day
                if (initialFromDuration > finalToDuration)
                {
                    log.Warn($"From normally shouldnt be grater than To: {correctFrom} - {correctTo}, even if it may happen in cross belts");
                    return result;
                }

                result.DayBelts.Add(new DayBeltChunkDTO()
                {
                    From = correctFrom,
                    To = correctTo,
                    TimeBelt = firstBelt.Item
                });
                result.IsCrossDayDetected = firstBelt.IsCrossDayBelt;


                log.Trace($"correctFrom: {correctFrom}, correctTo: {correctTo}");

                if(curBelts.Count > 1 && firstBelt.IsCrossDayBelt == false)
                {
                    //if there is only 1 element in curBelts this piece will fail, but there is no reason
                    var prevBeltTo = tmpFristBeltItemTo;
                    for (var i = firstBelt.Index + 1; i < curBelts.Count; i++)
                    {

                        //var timeToAdvance = prevBeltTo.Subtract(initialFromDuration);
                        //initialFromDuration = initialFromDuration.Add(timeToAdvance);
                        initialFromDuration = prevBeltTo;

                        if (initialFromDuration >= finalToDuration)
                        {
                            log.Debug("Previous was last useful belt");
                            break;
                        }

                        var correctCurBelt = GetFirstUsefulTimeBeltIndex(curBelts, initialFromDuration, finalToDuration);

                        result.DayBelts.Add(new DayBeltChunkDTO()
                        {
                            From = correctCurBelt.Item.From.ToTimeSpan(),
                            To = correctCurBelt.Item.To.ToTimeSpan() > finalToDuration ? finalToDuration : correctCurBelt.Item.To.ToTimeSpan(),
                            TimeBelt = correctCurBelt.Item
                        });

                        prevBeltTo = correctCurBelt.Item.To.ToTimeSpan();

                        if(correctCurBelt.IsCrossDayBelt == true)
                        {
                            log.Trace("It was CrossDayBelt, no reason go further");
                            result.IsCrossDayDetected = true;
                            //enough
                            break;
                        }

                    }
                }

            }

            return result;

        }
        #endregion

        /// <summary>
        /// Checks if given Rate and given Day is a Festivity/Special or Normal day
        /// </summary>
        /// <param name="CurRate"></param>
        /// <param name="Day"></param>
        /// <returns></returns>
        private static DayTypeEnum CheckDayType(Rate CurRate, DateTime Day)
        {

            //check if special or festivity day
            var festiviveDays = CurRate.ParkDaysOrFestivities.Where(x => x.Year == 0).ToList();
            var specialDays = CurRate.ParkDaysOrFestivities.Where(x => x.Year > 0).ToList();

            //check if its festivity
            var isFestivity = festiviveDays.Any(x =>
                x.Month == Day.Month
                &&
                x.Day == Day.Day
            );

            if (isFestivity == true)
            {
                return DayTypeEnum.Festivity;
            }

            //check also if its special day
            var isSpecialDay = specialDays.Any(x =>
                x.Year == Day.Year
                &&
                x.Month == Day.Month
                &&
                x.Day == Day.Day
            );

            if(isSpecialDay == true)
            {
                return DayTypeEnum.Special;
            }

            //if none cases are matched, its a common boring day
            return DayTypeEnum.Normal;

        }

        /// <summary>
        /// Returns WeekModel to use for passed Day 
        /// </summary>
        /// <param name="CurRate"></param>
        /// <param name="Day"></param>
        /// <returns></returns>
        private static WeekModel GetWeekModelToUse(Rate CurRate, DateTime Day)
        {
            if(CurRate.Exceptions != null && CurRate.Exceptions.Count > 0)
            {
                log.Debug("Some Exceptional WeekModelsFound");

                var curDay = Day.ToUniversalTime().ToTimestamp();

                var curEx = CurRate.Exceptions.Where(x => curDay >= x.From && curDay <= x.To).FirstOrDefault();

                if(curEx != null)
                {
                    log.Debug("Found Exceptional WeekModel");
                    return curEx.WeekModel;
                }
                else
                {
                    log.Debug("Anyway we dont match exceptions");
                    return CurRate.WeekModel;
                }

            }
            else
            {
                log.Debug("No exceptions defined");
                return CurRate.WeekModel;
            }
        }

        /// <summary>
        /// Gets DayModel for specific Day
        /// </summary>
        /// <param name="CurWeekModel"></param>
        /// <param name="DayType"></param>
        /// <param name="CurDayOfWeek"></param>
        /// <returns></returns>
        private static WeekModelDay GetWeekModelDay(WeekModel CurWeekModel, DayTypeEnum DayType, int CurDayOfWeek)
        {
            if (DayType == DayTypeEnum.Festivity)
            {
                return CurWeekModel.Days.Where(x => x.DayNumber == WeekModelDay.Types.DayNum.Festivity).FirstOrDefault();
            }
            else if (DayType == DayTypeEnum.Special)
            {
                return CurWeekModel.Days.Where(x => x.DayNumber == WeekModelDay.Types.DayNum.Special).FirstOrDefault();
            }
            else if (DayType == DayTypeEnum.Normal)
            {
                //here we need to know our week day
                return CurWeekModel.Days.Where(x => (int)x.DayNumber == CurDayOfWeek).FirstOrDefault();
            }
            else
            {
                log.Error("Not Managed DayType!");
                return null;
            }
        }

        /// <summary>
        /// Finds out correct starting rule for Belt based on priority passed and existing
        /// </summary>
        /// <param name="BeltChunk"></param>
        /// <param name="RCDTO"></param>
        /// <returns></returns>
        private static PaymentRule GetCorrectStartingRule(DayBeltChunkDTO BeltChunk, RateCalculationDTO RCDTO)
        {

            log.Trace("GetCorrectStartingRule Invoked!");

            var startingRule = BeltChunk.TimeBelt.PaymentRules.Where(x =>
                x.Priority == RCDTO.ReachedPriority
            ).FirstOrDefault();

            if (startingRule == null)
            {
                var adjustedPriority = Math.Min(BeltChunk.TimeBelt.PaymentRules.Max(x => x.Priority), RCDTO.ReachedPriority);
                log.Trace($"adjustedPriority {RCDTO.ReachedPriority} -> {adjustedPriority}");
                startingRule = BeltChunk.TimeBelt.PaymentRules.Where(x => x.Priority == adjustedPriority).FirstOrDefault();
            }

            return startingRule;

        }

        /// <summary>
        /// STILL TO FIX.
        /// Calculates Amount of current Time Belt proportionally to current day only
        /// </summary>
        private static RateCalculationDTO CalcProportionOfThisDayTBA(RateCalculationDTO CalculatedRate, DayBeltChunkDTO BeltChunk)
        {
            var gapToday = (new TimeSpan(24, 0, 0) - BeltChunk.From).TotalMinutes;
            var gapTomorrow = BeltChunk.To.TotalMinutes;
            
            //suppose for simplicity that cross day may have only 1 rule
            //var crossRule = BeltChunk.TimeBelt.PaymentRules.First();

            //calcultate percentage of Today respect of today+ tomorrow (whole belt)
            int percentComplete = (int)Math.Round((double)(100 * gapToday) / (gapToday + gapTomorrow));
            CalculatedRate.AmountToPay = (CalculatedRate.AmountToPay * percentComplete) / 100;

            log.Trace($"gapToday: {gapToday}, gapTomorrow: {gapTomorrow}, percentComplete: {percentComplete}, amountToPayToday: {CalculatedRate.AmountToPay}");

            return CalculatedRate;
        }

        /// <summary>
        /// Applies all rules progressively until gap is filled or last belt reached and processed
        /// The rest of gap will be processed subsequently with only last rule
        /// </summary>
        private static ConsumeTimeBelt_UntilLastRule_ResultDTO ConsumeTimeBelt_UntilLastRule(ConsumeTimeBeltDTO Data, double GapToFill)
        {

            //just init a returning object
            var result = new ConsumeTimeBelt_UntilLastRule_ResultDTO() { 
                GapToFill = GapToFill
            };

            //its first belt we are analyzing
            for (int i = Data.StartingRuleIndex; i < Data.BeltChunk.TimeBelt.PaymentRules.Count; i++)
            {

                Data.RateCalc.ReachedPriority = Data.BeltChunk.TimeBelt.PaymentRules[i].Priority;
                result.LastAppliedRule = Data.BeltChunk.TimeBelt.PaymentRules[i];

                var ruleMaxFrequency = (int)Math.Ceiling(
                    (double)Data.BeltChunk.TimeBelt.PaymentRules[i].Minutes
                    /
                    (double)Data.BeltChunk.TimeBelt.PaymentRules[i].Interval
                );

                var normalizedFrequency = (int)Math.Ceiling(
                    GapToFill
                    /
                    (double)Data.BeltChunk.TimeBelt.PaymentRules[i].Interval
                );

                var correctFreq = Math.Min(ruleMaxFrequency, normalizedFrequency);

                log.Trace($"ruleMaxFrequency: {ruleMaxFrequency}, normalizedFrequency: {normalizedFrequency}, correctFreq: {correctFreq}");

                //Transformed for loop in simple multiplication for efficiency reason (its the same)
                result.GapToFill -= Data.BeltChunk.TimeBelt.PaymentRules[i].Interval * correctFreq;
                result.AmountToPay += Data.BeltChunk.TimeBelt.PaymentRules[i].Amount * correctFreq;

                //To avoid loop through other rules if no need
                if (result.GapToFill <= 0)
                {
                    log.Trace("Processed enough to exit rule");
                    result.IsNeedStop = true;
                    return result;
                }

            }

            log.Trace("All rules applied, but Gap still needs to be full filled");

            return result;

        }

        /// <summary>
        /// Mainly Just process until final Amount to pay for current belt
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="DayIterPosEnum"></param>
        /// <param name="BeltIterPosEnum"></param>
        /// <returns></returns>
        private static ConsumeTimeBelt_UntilLastRule_ResultDTO ConsumeTimeBelt_UseLastRuleUntilEnd(ConsumeTimeBelt_UntilLastRule_ResultDTO Data, IterationPosEnum DayIterPosEnum, IterationPosEnum BeltIterPosEnum)
        {

            /////////
            //WARNING
            /////////
            //Here we may have a potential bug if Minutes and Interval are NOT proportional
            //Like: Minutes: 60, Interval: 7 -> we may calculate more or less to pay
            //As possible solution we may port excess to next calculation and do there some more logic
            if (Data.GapToFill > 0)
            {
                log.Trace("Still need processing by using last rule");
                //lets finish calulations by using just last rule, if no more
                while (Data.GapToFill > 0)
                {
                    Data.GapToFill -= Data.LastAppliedRule.Interval;
                    Data.AmountToPay += Data.LastAppliedRule.Amount;
                }

                //dont do this for first calcluation becouse probably those are free minutes
                bool isCanNotAdjustExceedingAmount = (
                    DayIterPosEnum != IterationPosEnum.Inner && BeltIterPosEnum != IterationPosEnum.Inner
                );

                log.Trace($"isCanNotAdjustExceedingAmount: {isCanNotAdjustExceedingAmount}");

                if (
                    Data.GapToFill < 0
                    &&
                    isCanNotAdjustExceedingAmount == false
                   )
                {
                    //do slight adjust, rectify calculation
                    //its already percentage of next belt (i think yes)? or of current?
                    //becouse initially gap fill is negative so it measures how much we gone far from current one
                    //in other words it means next belt shifting to correct
                    int percentComplete = (int)Math.Floor((double)(100 * -Data.GapToFill) / (double)Data.LastAppliedRule.Interval);
                    var correctionValue = (Data.LastAppliedRule.Amount * percentComplete) / 100;
                    Data.AmountToPay -= correctionValue;
                }

            }

            return Data;

        }

        /// <summary>
        /// Heart of Belt Consuming Algorithm
        /// </summary>
        /// <param name="BeltChunk"></param>
        /// <param name="StartingRuleIndex"></param>
        /// <param name="RateCalc"></param>
        /// <returns></returns>
        private static RateCalculationDTO ConsumeTimeBelt(ConsumeTimeBeltDTO Data)
        {

            log.Trace($"ConsumeTimeBelt Invoked! StartingRuleIndex: {Data.StartingRuleIndex}, PrevAmountToPay: {Data.RateCalc.AmountToPay}");

            //in Minutes becouse whole server side logic is detailed to minute
            var gapToFill = Data.BeltChunk.To.Subtract(Data.BeltChunk.From).TotalMinutes;

            //Potentially we can decide if its cross day even with underlying if
            bool isCrossDay = false;
            //in case of cross date
            if(gapToFill < 0) {
                log.Trace($"Rotated gapToFill: {gapToFill}, its probably a cross day");
                //1440 => 24h in minutes, its needed to rotate appropriately
                gapToFill = 1440 + gapToFill;
                isCrossDay = true;
            }

            var firstPass = ConsumeTimeBelt_UntilLastRule(Data, gapToFill);
            if(firstPass.IsNeedStop == true)
            {
                Data.RateCalc.AmountToPay += firstPass.AmountToPay;
                log.Trace("Stop required after first pass");
                return Data.RateCalc;
            }

            var secondPass = ConsumeTimeBelt_UseLastRuleUntilEnd(firstPass, Data.DayIterPosEnum, Data.BeltIterPosEnum);
            Data.RateCalc.AmountToPay += secondPass.AmountToPay;

            if (isCrossDay == true)
            {
                log.Trace("Detected CorssDay Belt!");
                //calculate proportion belonging to this day
                //Data.RateCalc = CalcProportionOfThisDayTBA(Data.RateCalc, Data.BeltChunk);
            }

            return Data.RateCalc;

        }

        /// <summary>
        /// Wil calculate amount to pay for specific Belt based on eventual previous calculations
        /// </summary>
        /// <param name="BeltChunk"></param>
        /// <param name="PrevRateCalc"></param>
        /// <returns></returns>
        private static RateCalculationDTO ProcessBelt(ProcessBeltDTO Data)
        {

            log.Debug($"--- ProcessBelt Invoked! {Data.BeltChunk.TimeBelt.Name}");

            //if passed prevClac is null, it means we need strt new calculation, else just use previous data and expand amount
            var result = new RateCalculationDTO()
            {
                ReachedPriority = Data.PreviouslyReachedPriority
            };

            //We suppose that priorities are already also ordered

            var startingRule = GetCorrectStartingRule(Data.BeltChunk, result);

            //optimizable with previous instruction
            var startingRuleIndex = Data.BeltChunk.TimeBelt.PaymentRules.IndexOf(startingRule);

            result = ConsumeTimeBelt(new ConsumeTimeBeltDTO() {
                BeltChunk = Data.BeltChunk,
                StartingRuleIndex = startingRuleIndex,
                RateCalc = result,
                DayIterPosEnum = Data.DayIterPosEnum,
                BeltIterPosEnum = Data.BeltIterPosEnum
            });

            log.Trace($"--- Individual amount to pay after processing: {result.AmountToPay}");

            return result;

        }

        /// <summary>
        /// Checks if we are under free minutes or if we can use precalculated day amount
        /// Just to make code all readable, its first part of ProcessDay function
        /// </summary>
        /// <param name="CurDay"></param>
        /// <param name="CurModelDay"></param>
        /// <param name="PrevRateCalc"></param>
        /// <returns></returns>
        private static (bool IsReturnable, RateCalculationDTO Calculation) PerformInitialChecks_ProcessDay(PeriodDTO CurDay, WeekModelDay CurModelDay, RateCalculationDTO PrevRateCalc)
        {
            //if first calculation
            if (PrevRateCalc == null)
            {
                //subtract free minutes anyway, from first day, cause the shouldnt be calculated
                //becouse thy are free!
                CurDay.From = CurDay.From.AddMinutes(CurModelDay.InitialFreeMinutes);
                if (CurDay.From >= CurDay.To)
                {
                    log.Debug("We Are inside free minutes! Wont do calculations");
                    return (true, new RateCalculationDTO()
                    {
                        AmountToPay = 0
                    });
                }
            }
            //once reached maximum of first day, we will reach maximum in every next day, except last one 
            //this will increase hugely performance for long period calculations
            //normally last day will have From != To
            //Even if its not the case there is no problem, just give max precalculated amount
            //and dont do this check first day, so it became else if
            else if (
                PrevRateCalc != null && PrevRateCalc.AmountToPay > CurModelDay.MaxPrecalculatedDayAmount
                &&
                CurDay.From.AddDays(1) == CurDay.To
                )
            {
                log.Debug($"Will use precalculated value for day: {CurDay.From}-{CurDay.To} => {CurModelDay.MaxPrecalculatedDayAmount}");
                PrevRateCalc.AmountToPay += CurModelDay.MaxPrecalculatedDayAmount;
                return (true, PrevRateCalc);
            }

            log.Trace("PerformInitialChecks_ProcessDay passed regularly");

            //In this case second parameter just doesnt count
            //eventually it might be replaced with: PrevRateCalc
            return (false, null);

        }

        /// <summary>
        /// Does Real Rate Calculation Logic
        /// </summary>
        /// <param name="CurDay"></param>
        /// <param name="CurModelDay"></param>
        /// <returns></returns>
        private static RateCalculationDTO ProcessDay(PeriodDTO CurDay, WeekModelDay CurModelDay, RateCalculationDTO PrevRateCalc, IterationPosEnum DayIterPosEnum)
        {
            log.Debug($"ProcessDay Invoked! From: {CurDay.From}, To: {CurDay.To}");
            
            var result = PrevRateCalc ?? new RateCalculationDTO();

            var checkRes = PerformInitialChecks_ProcessDay(CurDay, CurModelDay, PrevRateCalc);
            if(checkRes.IsReturnable == true)
            {
                return checkRes.Calculation;
            }

            //chunk day in its Belts
            var beltedDay = ChunkDayInBelts(CurDay, CurModelDay, PrevRateCalc);

            log.Trace($"Day chunked in: {beltedDay.DayBelts.Count} belts");

            RateCalculationDTO tmpResult = new RateCalculationDTO();
            if(PrevRateCalc != null)
            {
                tmpResult.ReachedPriority = PrevRateCalc.ReachedPriority;
            }

            //Each result collected before is passed to next processing belt
            //In the end we will have day Amount on last iteration
            for(var i = 0; i < beltedDay.DayBelts.Count; i++)
            {

                var beltIterPosEnum = GetIterationPosEnum(i, beltedDay.DayBelts.Count);

                //Cumulative logic
                //if its first entry, result is equals null, but its ok
                var latestResult = ProcessBelt(new ProcessBeltDTO() {
                    BeltChunk = beltedDay.DayBelts[i],
                    DayIterPosEnum = DayIterPosEnum,
                    BeltIterPosEnum = beltIterPosEnum,
                    PreviouslyReachedPriority = tmpResult.ReachedPriority
                });

                //this check is needed in case single belt may overcome whole day max limit
                //we rarely or never will pass here, at least if Rates model is built well
                if(CurModelDay.MaxDailyRate > 0 && latestResult.AmountToPay >= CurModelDay.MaxDailyRate)
                {
                    latestResult.AmountToPay = CurModelDay.MaxDailyRate;
                    tmpResult.MergeWithSimilar(latestResult);
                    log.Trace("Max daily Rate Reached, wont do further for this day");
                    break;
                }

                tmpResult.MergeWithSimilar(latestResult);

            }

            //Now check that whole sum is still lower tha max sum
            if (CurModelDay.MaxDailyRate > 0 && tmpResult.AmountToPay > CurModelDay.MaxDailyRate)
            {
                tmpResult.AmountToPay = CurModelDay.MaxDailyRate;
                log.Trace("Global Belt Max daily Rate Reached, wont do further for this day");
            }

            //final assignation
            result.MergeWithSimilar(tmpResult);

            //This chec will ensure that we will propagate correctly first detected cross day until last day
            //So in last calculation we will perform additional logic based on this boolean
            result.IsCrossDayDetected = (
                (PrevRateCalc != null && PrevRateCalc.IsCrossDayDetected)
                ||
                beltedDay.IsCrossDayDetected
                );

            return result;

        }

        /// <summary>
        /// Simply returns iteration position.
        /// Mainly needed to understand if current element is first / inner or last.
        /// </summary>
        /// <returns></returns>
        private static IterationPosEnum GetIterationPosEnum(int curPos, int TotItems)
        {
            if(TotItems == 1)
            {
                return IterationPosEnum.FirstAndLast;
            }
            else if(curPos == 0)
            {
                return IterationPosEnum.First;
            }
            else if (curPos == TotItems - 1)
            {
                return IterationPosEnum.Last;
            }
            else
            {
                return IterationPosEnum.Inner;
            }
        }

        /// <summary>
        /// Calculates amount to pay based on local rate descriptor
        /// </summary>
        /// <param name="From">Start of calculation</param>
        /// <param name="To">Stop of calculation</param>
        /// <returns></returns>
        public static long CalculateAmountToPay(this Rate CurRate, DateTime From, DateTime To)
        {
            try
            {

                log.Trace("----------------START RATE CALC---------------------");
                log.Debug($"CalculateAmountToPay Invoked! Period: {From} - {To}");
                
                //Cast to Universal time becouse Rates are recorded in universal time format
                //This is just to avoind many castings in future steps
                var everySingleDay = ChunkInSingleDayPeriods(new PeriodDTO() 
                {
                    From = From,
                    To = To
                });

                log.Trace($"Period chunked in: {everySingleDay.Count} days");

                RateCalculationDTO lastCalculation = null;
                for (var i = 0; i < everySingleDay.Count; i++)
                {

                    var curWeekModel = GetWeekModelToUse(CurRate, everySingleDay[i].From);

                    var dayType = CheckDayType(CurRate, everySingleDay[i].From);

                    var curWeekModelDay = GetWeekModelDay(curWeekModel, dayType, (int)everySingleDay[i].From.DayOfWeek);

                    log.Trace($"--- Day to process: {everySingleDay[i].From} - {everySingleDay[i].To}, WeekModelName: {curWeekModel.WeekModelName}, DayType: {dayType}, CurWeekModelDayNumber: {curWeekModelDay.DayNumber}");

                    var iterationPos = GetIterationPosEnum(i, everySingleDay.Count);

                    //Cumulative approach
                    lastCalculation = ProcessDay(everySingleDay[i], curWeekModelDay, lastCalculation, iterationPos);

                    log.Trace($"--- Day processed: {lastCalculation.ToString()}");

                }

                log.Debug($"Final Amount to pay: {lastCalculation.AmountToPay}");
                log.Trace("----------------STOP RATE CALC---------------------");

                return lastCalculation.AmountToPay;

            }
            catch(Exception ex)
            {
                log.Error(ex);
                return -1;
            }
        }

        ////////////////////////////////////////
        //These methods are out of Payment Logic
        ////////////////////////////////////////
        #region Other Util Methods

        /// <summary>
        /// Gets ParkMode by passed date and considering current Rate
        /// </summary>
        /// <param name="CurRate"></param>
        /// <param name="Time"></param>
        /// <returns></returns>
        public static int GetParkMode(this Rate CurRate, DateTime Time)
        {
            try
            {

                log.Debug($"GetParkMode Invoked! {Time}");

                var myTime = Time.ToUniversalTime().ToTimestamp();

                WeekModel modelToConsider = null;
                
                var isException = CurRate.Exceptions.Where(x => x.From >= myTime && x.To < myTime).FirstOrDefault()?.WeekModel;
                if(isException == null)
                {
                    modelToConsider = CurRate.WeekModel;
                }
                else
                {
                    modelToConsider = isException;
                }

                var dayType = CheckDayType(CurRate, Time);
                var curDay = GetWeekModelDay(modelToConsider, dayType, (int)Time.DayOfWeek);

                var fromT = Time.TimeOfDay;
                var toT = Time.TimeOfDay.Add(new TimeSpan(0, 0, 1));
                var goodTime = GetFirstUsefulTimeBeltIndex(curDay.TimeBelts.ToList(), fromT, toT);

                var curBelt = curDay.TimeBelts[goodTime.Index];

                return curBelt.ParkState;

            }
            catch(Exception ex)
            {
                log.Error(ex);
                return 0;
            }
        }

        #endregion

    }
}
