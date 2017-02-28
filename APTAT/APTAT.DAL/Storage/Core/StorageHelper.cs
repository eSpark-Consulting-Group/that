using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Common;

namespace APTAT.DAL.Storage.Core
{
    internal static class StorageHelper
    {
        public static int SaveChangesWithErrorsHandling(this DbContext context, ConcurrencyResolutionStrategy strategy)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            int concurrencyExceptionCounter = 0;

            while (true)
            {
                try
                {
                    int result = context.SaveChanges();

                    if (concurrencyExceptionCounter > 0)
                        LogHelper.Trace(TraceEventType.Warning, "Concurrency update issue was succesfully resolved. Tries: {0}", concurrencyExceptionCounter);

                    return result;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    concurrencyExceptionCounter++;
                    if (!HandleConcurrencyException(ex, context, strategy, concurrencyExceptionCounter))
                    {
                        throw;
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationResult in ex.EntityValidationErrors)
                    {
                        LogValidationError(validationResult);
                    }
                    throw;
                }
            }
        }

        private static bool HandleConcurrencyException(
            DbUpdateConcurrencyException exception,
            DbContext context,
            ConcurrencyResolutionStrategy strategy,
            int exceptionCounter)
        {
            LogHelper.Trace(TraceEventType.Verbose, "Concurrency issue stack trace:\n{0}", new StackTrace(true));

            bool solved;

            if (exceptionCounter < ConcurrencyRetriesMaxNumber)
            {
                switch (strategy)
                {
                    case ConcurrencyResolutionStrategy.ThrowException:
                        solved = false;
                        break;
                    case ConcurrencyResolutionStrategy.StoreWins:
                        ApplyStoreWinsStrategy(exception);
                        solved = true;
                        break;
                    case ConcurrencyResolutionStrategy.ClientWins:
                        ApplyClientWinsStrategy(exception, context);
                        solved = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("strategy");
                }
            }
            else
            {
                solved = false;
            }

            return solved;
        }

        private static void ApplyStoreWinsStrategy(DbUpdateConcurrencyException exception)
        {
            // Update the values of the entity that failed to save from the store
            foreach (var entry in exception.Entries)
            {
                entry.Reload();
            }
        }

        private static void ApplyClientWinsStrategy(DbUpdateConcurrencyException exception, DbContext context)
        {
            foreach (var entry in exception.Entries)
            {
                switch (entry.State)
                {
                    case EntityState.Deleted:

                        entry.Reload();
                        if (entry.State == EntityState.Detached)
                        {
                            //Deleted on client, deleted in store
                            //Do nothing
                        }
                        else
                        {
                            //Deleted on client, modified in store
                            entry.State = EntityState.Deleted;
                        }
                        break;
                    case EntityState.Modified:
                        DbPropertyValues currentValues = entry.CurrentValues.Clone();
                        entry.Reload();
                        if (entry.State == EntityState.Detached)
                        {
                            //Modified on client, deleted in store
                            context.Set(ObjectContext.GetObjectType(entry.Entity.GetType())).Add(entry.Entity);
                        }
                        else
                        {
                            //Modified on client, Modified in store
                            entry.Reload();
                            entry.CurrentValues.SetValues(currentValues);
                        }
                        break;
                    default:
                        entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                        break;
                }
            }
        }

        private static int ConcurrencyRetriesMaxNumber
        {
            get { return 10; }
        }

        internal static void LogValidationError(DbEntityValidationResult validationResult)
        {
            var sb = new StringBuilder();
            var entry = validationResult.Entry;
            var indent = "";
            sb.AppendFormat("{0}Saving {1} entity of type '{2}' {3} with {4} validation error(s):\n",
                indent,
                entry.State.ToString().ToLower(),
                entry.Entity.GetType().Name,
                validationResult.IsValid ? "succeeded" : "failed",
                validationResult.ValidationErrors.Count);
            indent = "    ";
            sb.AppendLine();
            sb.AppendFormat("{0}Properties in error:\n{0}------\n", indent);
            foreach (var error in validationResult.ValidationErrors)
                sb.AppendFormat("{0}{1}: {2}\n", indent, error.PropertyName, error.ErrorMessage);
            sb.AppendLine();
            if (entry.State == EntityState.Modified)
            {
                sb.AppendFormat("{0}Original values:\n{0}------\n", indent);
                foreach (var property in entry.OriginalValues.PropertyNames)
                {
                    var value = entry.OriginalValues[property];
                    if (value is byte[])
                        value = BitConverter.ToString((byte[])value);
                    sb.AppendFormat("{0}{1} = {2}\n", indent, property, value == null || value is DBNull ? "(null)" : "'" + value + "'");
                }
                sb.AppendLine();
            }
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                sb.AppendFormat("{0}Current values:\n{0}------\n", indent);
                foreach (var property in entry.CurrentValues.PropertyNames)
                {
                    var value = entry.CurrentValues[property];
                    if (value is byte[])
                        value = BitConverter.ToString((byte[])value);
                    sb.AppendFormat("{0}{1} = {2}\n", indent, property, value == null || value is DBNull ? "(null)" : "'" + value + "'");
                }
            }
            LogHelper.Trace(TraceEventType.Verbose, sb.ToString());
        }

        //public static void RetryIfDeadlock(Action actionToRetry, StoreProvider storeProvider)
        //{
        //    if (actionToRetry == null) throw new ArgumentNullException("actionToRetry");
        //    if (storeProvider == null) throw new ArgumentNullException("storeProvider");

        //    int exceptionCounter = 0;
        //    Random random = null;

        //    while (true)
        //    {
        //        try
        //        {
        //            actionToRetry();

        //            if (exceptionCounter > 0)
        //                LogHelper.Trace(TraceEventType.Warning, "Deadlock issue was succesfully resolved. Tries: {0}", exceptionCounter);
        //            break;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (random == null)
        //                random = new Random();

        //            exceptionCounter++;
        //            if (!HandleDeadlockException(ex, random, exceptionCounter, storeProvider))
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}





        

        //private static bool HandleDeadlockException(Exception exception, Random random, int exceptionCounter, StoreProvider storeProvider)
        //{
        //    bool result = false;
        //    if (storeProvider == null) throw new ArgumentNullException("storeProvider");
        //    if (IsDeadlockException(exception, storeProvider) && exceptionCounter < DeadlockRetriesMaxNumber)
        //    {
        //        LogErrorSolvingMessage("A database deadlock (or lock timeout) error detected");

        //        var minValue = 100 * exceptionCounter;
        //        var maxValue = minValue * 2;
        //        var millisecondsTimeout = random.Next(minValue, maxValue);

        //        Thread.Sleep(millisecondsTimeout);
        //        result = true;
        //    }

        //    return result;
        //}

        //private static int DeadlockRetriesMaxNumber
        //{
        //    get { return 5; }
        //}

        //private static bool IsDeadlockException(Exception exception, StoreProvider storeProvider)
        //{
        //    if (exception == null) throw new ArgumentNullException("exception");
        //    if (storeProvider == null) throw new ArgumentNullException("storeProvider");

        //    if (storeProvider.IsLockTimeoutException(exception))
        //    {
        //        return true;
        //    }
        //    if (exception.InnerException != null)
        //        return IsDeadlockException(exception.InnerException, storeProvider);
        //    return false;
        //}

        //private static void LogErrorSolvingMessage(string exceptionMessage)
        //{
        //    LogHelper.Trace(TraceEventType.Verbose, "{0}. Trying to solve the problem...", exceptionMessage);
        //}
    }
}
