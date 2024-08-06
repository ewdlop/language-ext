﻿using System;
using LanguageExt.Common;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Trait implementation for `EitherT` 
/// </summary>
/// <typeparam name="M">Given monad trait</typeparam>
public partial class FinT<M> : 
    MonadT<FinT<M>, M>, 
    Fallible<FinT<M>>,
    SemiAlternative<FinT<M>>
    where M : Monad<M>
{
    static K<FinT<M>, B> Monad<FinT<M>>.Bind<A, B>(K<FinT<M>, A> ma, Func<A, K<FinT<M>, B>> f) => 
        ma.As().Bind(f);

    static K<FinT<M>, B> Functor<FinT<M>>.Map<A, B>(Func<A, B> f, K<FinT<M>, A> ma) => 
        ma.As().Map(f);

    static K<FinT<M>, A> Applicative<FinT<M>>.Pure<A>(A value) => 
        FinT<M, A>.Succ(value);

    static K<FinT<M>, B> Applicative<FinT<M>>.Apply<A, B>(K<FinT<M>, Func<A, B>> mf, K<FinT<M>, A> ma) => 
        mf.As().Bind(ma.As().Map);

    static K<FinT<M>, B> Applicative<FinT<M>>.Action<A, B>(K<FinT<M>, A> ma, K<FinT<M>, B> mb) =>
        ma.As().Bind(_ => mb);

    static K<FinT<M>, A> MonadT<FinT<M>, M>.Lift<A>(K<M, A> ma) => 
        FinT<M, A>.Lift(ma);
        
    static K<FinT<M>, A> MonadIO<FinT<M>>.LiftIO<A>(IO<A> ma) => 
        FinT<M, A>.Lift(M.LiftIO(ma));

    static K<FinT<M>, A> SemigroupK<FinT<M>>.Combine<A>(K<FinT<M>, A> ma, K<FinT<M>, A> mb) =>
        new FinT<M, A>(
            M.Bind(ma.As().runFin,
                   ea => ea switch
                         {
                             Fin.Succ<A> => M.Pure(ea),
                             Fin.Fail<A> => mb.As().runFin,
                             _           => M.Pure(ea)
                         }));

    static K<FinT<M>, A> Fallible<Error, FinT<M>>.Fail<A>(Error error) =>
        Fail<A>(error);

    static K<FinT<M>, A> Fallible<Error, FinT<M>>.Catch<A>(
        K<FinT<M>, A> fa, Func<Error, bool> Predicate,
        Func<Error, K<FinT<M>, A>> Fail) =>
        fa.As().BindFail(l => Predicate(l) ? Fail(l).As() : Fail<A>(l));
}
