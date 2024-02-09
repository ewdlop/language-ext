using System;
using System.IO;
using System.Threading.Tasks;
using LanguageExt.Common;
using LanguageExt.HKT;
using static LanguageExt.Prelude;
using static LanguageExt.Transducer;

namespace LanguageExt;

public static class Testing
{
    public static void Test1()
    {
        var m1 = ReaderT<string, Maybe, int>.Lift(Maybe<int>.Just(123));
        var m2 = ReaderT<string, Maybe, int>.Lift(Maybe<int>.Just(123));
        
        var m3 = from w in Pure(123)
                 from x in m1
                 from r in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from y in m2
                 from z in Maybe<int>.Just(100)
                 from e in ask<string>()
                 from _ in release(r)
                 select $"{e}: {w + x + y + z}";

        var r1 = m3.Run("Hello");
        
        var m4 = from x in m1
                 from y in m2
                 from z in Maybe<int>.Nothing
                 from e in ask<string>()
                 select $"{e}: {x + y + z}";

        var r2 = m4.Run("Hello");
    }
    
    public static void Test2()
    {
        var m1 = Reader<string, int>.Pure(123);
        var m2 = Reader<string, int>.Pure(123);
        
        var m3 = from x in m1
                 from y in m2
                 from e in ask<string>()
                 select $"{e}: {x + y}";
        
        var m4 = from x in m1
                 from y in m2
                 from e in ask<string>()
                 from z in Pure(234)
                 select $"{e}: {x + y}";
    }
    
    public static void Test3()
    {
        var m1 = ReaderT<string, Either<string>, int>.Lift(Right(123));
        var m2 = ReaderT<string, Either<string>, int>.Lift(Right(123));
        
        var m3 = from w in Pure(123)
                 from x in m1
                 from r in use(() => File.Open("c:\\test.txt", FileMode.Open))
                 from y in m2
                 from z in Right(100)
                 from e in ask<string>()
                 from _ in release(r)
                 select $"{e}: {w + x + y + z}";

        var r1 = m3.Run("Hello");
        
        var m4 = from x in m1
                 from y in m2
                 from z in Left<string, int>("fail")
                 from e in ask<string>()
                 select $"{e}: {x + y + z}";

        var r2 = m4.Run("Hello");        
    }
    
    
    public static async Task Test4()
    {
        var m1 = App<string, int>.Pure(123);
        var m2 = App<string, int>.Pure(123);
        var m3 = App<string, int>.Fail(Error.New("fail"));
        
        var m4 = from w in Pure(234)
                 from x in m1
                 from y in m2
                 from z in m3
                 from t in liftAsync(async () => await File.ReadAllTextAsync("c:\\test.txt"))
                 from e in ask<string>()
                 select $"{t} {e}: {w + x + y + z}";

        var r1 = await m4.RunAsync("Hello");
    }
    
}

public class Maybe : Monad<Maybe>
{
    public static Monad<Maybe, A> Pure<A>(A value) => 
        Maybe<A>.Just(value);

    public static Monad<Maybe, B> Bind<A, B>(Monad<Maybe, A> ma, Transducer<A, Monad<Maybe, B>> f) => 
        ma.As().Bind(f.Map(mb => mb.As()));

    public static Functor<Maybe, B> Map<A, B>(Functor<Maybe, A> ma, Transducer<A, B> f) => 
        ma.As().Bind(f.Map(Maybe<B>.Just));
}

public record Maybe<A>(Transducer<Unit, Sum<Unit, A>> M) : Monad<Maybe, A>
{
    public Transducer<Unit, Sum<Unit, A>> Morphism { get; } = M;

    public static Maybe<A> Just(A value) =>
        new(constant<Unit, Sum<Unit, A>>(Sum<Unit, A>.Right(value)));
    
    public static readonly Maybe<A> Nothing = 
        new(constant<Unit, Sum<Unit, A>>(Sum<Unit, A>.Left(default)));

    public Maybe<B> Bind<B>(Func<A, Maybe<B>> f) =>
        Bind(lift(f));

    public Maybe<B> Bind<B>(Transducer<A, Maybe<B>> f) =>
        new(mapRight(M, f.Map(b => b.Morphism)).Flatten());
}

public static class MaybeExt
{
    public static Maybe<A> As<A>(this Monad<Maybe, A> ma) =>
        (Maybe<A>)ma;
    
    public static Maybe<A> As<A>(this Functor<Maybe, A> ma) =>
        (Maybe<A>)ma;
}

public record App<Env, A>(Transducer<Env, Monad<Either<Error>, A>> RunReader)
    : ReaderT<Env, Either<Error>, A>(RunReader)
{
    public static ReaderT<Env, Either<Error>, A> Fail(Error error) =>
        Lift(Left<Error, A>(error));
}

