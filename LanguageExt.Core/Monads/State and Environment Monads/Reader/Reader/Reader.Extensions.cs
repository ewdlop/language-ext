﻿using static LanguageExt.Prelude;
using System.Diagnostics.Contracts;
using LanguageExt.Traits;

namespace LanguageExt;

/// <summary>
/// Reader monad extensions
/// </summary>
public static partial class ReaderExtensions
{
    public static Reader<Env, A> As<Env, A>(this K<Reader<Env>, A> ma) =>
        (Reader<Env, A>)ma;
    
    /// <summary>
    /// Monadic join
    /// </summary>
    [Pure]
    public static Reader<Env, A> Flatten<Env, A>(this Reader<Env, Reader<Env, A>> mma) =>
        mma.Bind(identity);
}
