using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace App
{
    /// <summary>
    /// Taken from: https://github.com/habaneroofdoom/AltNetRop
    /// </summary>
    public class Result<TSuccess, TFailure>
    {
        public static Result<TSuccess, TFailure> Succeeded(TSuccess success)
        {
            if (success == null) throw new ArgumentNullException(nameof(success));

            return new Result<TSuccess, TFailure>
            {
                IsSuccessful = true,
                Success = success
            };
        }

        public static Result<TSuccess, TFailure> Failed(TFailure failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));

            return new Result<TSuccess, TFailure>
            {
                IsSuccessful = false,
                Failure = failure
            };
        }

        public static implicit operator Result<TSuccess, TFailure>(TSuccess success)
        {
            return Result<TSuccess, TFailure>.Succeeded(success);
        }

        public static implicit operator Result<TSuccess, TFailure>(TFailure failure)
        {
            return Result<TSuccess, TFailure>.Failed(failure);
        }

        private Result()
        {
        }

        public bool IsSuccess => IsSuccessful;

        public bool IsFailure => !IsSuccessful;

        public TSuccess Success { get; private set; }
        public TFailure Failure { get; private set; }
        private bool IsSuccessful { get; set; }
    }


    /// <summary>
    /// Taken from on: https://github.com/habaneroofdoom/AltNetRop (Sync)
    /// </summary>
    public static class ResultExtensions
    {
        public static void Handle<TSuccess, TFailure>(this Result<TSuccess, TFailure> result,
            Action<TSuccess> onSuccess,
            Action<TFailure> onFailure)
        {
            if (result.IsSuccess)
                onSuccess(result.Success);
            else
                onFailure(result.Failure);
        }

        public static Result<TSuccess2, TFailure2> Either<TSuccess, TFailure, TSuccess2, TFailure2>(
            this Result<TSuccess, TFailure> x,
            Func<Result<TSuccess, TFailure>, Result<TSuccess2, TFailure2>> onSuccess,
            Func<Result<TSuccess, TFailure>, Result<TSuccess2, TFailure2>> onFailure)
        {
            return x.IsSuccess ? onSuccess(x) : onFailure(x);
        }

        public async static Task<Result<TSuccessNew, TFailureNew>> Map<TSuccess, TFailure, TSuccessNew, TFailureNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, TSuccessNew> onSuccess,
            Func<TFailure, TFailureNew> onFailure)
        {
            var v = await x;
            return v.Map(onSuccess, onFailure);
        }

        public static Result<TSuccessNew, TFailureNew> Map<TSuccess, TFailure, TSuccessNew, TFailureNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, TSuccessNew> onSuccess,
            Func<TFailure, TFailureNew> onFailure)
        {
            return x.IsSuccess
                ? Result<TSuccessNew, TFailureNew>.Succeeded(onSuccess(x.Success))
                : Result<TSuccessNew, TFailureNew>.Failed(onFailure(x.Failure));
        }

        // Whatever x is, make it a failure.
        // The trick is that failure is an array type, can it can be made an empty array failure.
        public static Result<TSuccess, TFailure[]> ToFailure<TSuccess, TFailure>(
            this Result<TSuccess, TFailure[]> x)
        {
            return x.Either(
                a => Result<TSuccess, TFailure[]>.Failed(new TFailure[0]),
                b => b
                );
        }

        // Put accumulator and next together.
        // If they are both successes, then put them together as a success.
        // If either/both are failures, then put them together as a failure.
        // Because success and failure is an array, they can be put together
        public static Result<TSuccess[], TFailure[]> Merge<TSuccess, TFailure>(
            this Result<TSuccess[], TFailure[]> accumulator,
            Result<TSuccess, TFailure[]> next)
        {
            if (accumulator.IsSuccess && next.IsSuccess)
            {
                return Result<TSuccess[], TFailure[]>
                    .Succeeded(accumulator.Success.Concat(new List<TSuccess>() { next.Success })
                        .ToArray());
            }
            return Result<TSuccess[], TFailure[]>
                .Failed(accumulator.ToFailure().Failure.Concat(next.ToFailure().Failure).ToArray());
        }

        // Aggregate an array of results together.
        // If any of the results fail, return combined failures
        // Will only return success if all results succeed
        public static Result<TSuccess[], TFailure[]> Aggregate<TSuccess, TFailure>(
            this IEnumerable<Result<TSuccess, TFailure[]>> accumulator)
        {
            var emptySuccess = Result<TSuccess[], TFailure[]>.Succeeded(new TSuccess[0]);
            return accumulator.Aggregate(emptySuccess, (acc, o) => acc.Merge(o));
        }

        // Map: functional map
        // if x is a a success call f, otherwise pass it through as a failure
        public static Result<TSuccessNew, TFailure> Map<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, TSuccessNew> f)
        {
            return x.IsSuccess
                ? Result<TSuccessNew, TFailure>.Succeeded(f(x.Success))
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        // Bind: functional bind
        // Monadize it!
        public static Result<TSuccessNew, TFailure> Bind<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, Result<TSuccessNew, TFailure>> f)
        {
            return x.IsSuccess
                ? f(x.Success)
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Bind<TSuccess, TFailure, TSuccessNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, Result<TSuccessNew, TFailure>> f)
        {
            return (await x).Bind(f);
        }

        public static Result<TSuccess, TFailure> Tee<TSuccess, TFailure>(this Result<TSuccess, TFailure> x, Action<TSuccess> f)
        {
            if (x.IsSuccess)
            {
                f(x.Success);
            }

            return x;
        }

        public async static Task<Result<TSuccess, TFailure>> Tee<TSuccess, TFailure>(this Result<TSuccess, TFailure> x, Func<TSuccess, Task> f)
        {
            if (x.IsSuccess)
            {
                await f(x.Success);
            }

            return x;
        }

        public async static Task<Result<TSuccess, TFailure>> Tee<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> x, Func<TSuccess, Task> f)
        {
            Result<TSuccess, TFailure> result = await x;
            if (result.IsSuccess)
            {
                await f(result.Success);
            }

            return result;
        }

        public async static Task<Result<TSuccess, TFailure>> Tee<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> x, Action<TSuccess> f)
        {
            Result<TSuccess, TFailure> result = await x;
            if (result.IsSuccess)
            {
                f(result.Success);
            }

            return result;
        }

        public static async Task Handle<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> result,
            Action<TSuccess> onSuccess,
            Action<TFailure> onFailure)
        {
            (await result).Handle(onSuccess, onFailure);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Bind<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, Task<Result<TSuccessNew, TFailure>>> f)
        {
            return x.IsSuccess
                ? await f(x.Success)
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Bind<TSuccess, TFailure, TSuccessNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, Task<Result<TSuccessNew, TFailure>>> f)
        {
            return await Bind(await x, f);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Map<TSuccess, TFailure, TSuccessNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, TSuccessNew> f)
        {
            return Map(await x, f);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Map<TSuccess, TFailure, TSuccessNew>(
            this Result<TSuccess, TFailure> x,
            Func<TSuccess, Task<TSuccessNew>> f)
        {
            return x.IsSuccess
                ? Result<TSuccessNew, TFailure>.Succeeded(await f(x.Success))
                : Result<TSuccessNew, TFailure>.Failed(x.Failure);
        }

        public static async Task<Result<TSuccessNew, TFailure>> Map<TSuccess, TFailure, TSuccessNew>(
            this Task<Result<TSuccess, TFailure>> x,
            Func<TSuccess, Task<TSuccessNew>> f)
        {
            Result<TSuccess, TFailure> result = await x;

            return result.IsSuccess
                ? Result<TSuccessNew, TFailure>.Succeeded(await f(result.Success))
                : Result<TSuccessNew, TFailure>.Failed(result.Failure);
        }

        public static TSuccessAndFailure Either<TSuccessAndFailure>(this Result<TSuccessAndFailure, TSuccessAndFailure> finalResult)
        {
            return finalResult.IsSuccess ? finalResult.Success : finalResult.Failure;
        }

        public static async Task<TSuccessAndFailure> Either<TSuccessAndFailure>(this Task<Result<TSuccessAndFailure, TSuccessAndFailure>> finalResult)
        {
            return (await finalResult).Either();
        }
    }
}