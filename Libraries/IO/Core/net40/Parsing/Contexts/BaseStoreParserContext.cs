/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Base Class for Store Parser Contexts
    /// </summary>
    public abstract class BaseStoreParserContext : IStoreParserContext
    {
        /// <summary>
        /// Is Parsing Traced?
        /// </summary>
        protected bool _traceParsing = false;

        private IRdfHandler _handler;
        private NamespaceMapper _nsmap = new NamespaceMapper(true);
        private Uri _baseUri;

        /// <summary>
        /// Creates a new Store Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseStoreParserContext(IRdfHandler handler, bool traceParsing)
        {
            if (handler == null) throw new ArgumentNullException("handler", "RDF Handler cannot be null");
            this._handler = handler;
            this._traceParsing = traceParsing;
        }

        /// <summary>
        /// Creates a new Store Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        public BaseStoreParserContext(IRdfHandler handler)
            : this(handler, false) { }

        /// <summary>
        /// Creates a new Base Store Parser Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        public BaseStoreParserContext(ITripleStore store)
            : this(new StoreHandler(store)) { }

        /// <summary>
        /// Creates a new Base Parser Context
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        public BaseStoreParserContext(ITripleStore store, bool traceParsing)
            : this(new StoreHandler(store), traceParsing) { }

        /// <summary>
        /// Gets/Sets whether to trace parsing
        /// </summary>
        public bool TraceParsing
        {
            get
            {
                return this._traceParsing;
            }
            set
            {
                this._traceParsing = value;
            }
        }

        /// <summary>
        /// Gets the RDF Handler that is in-use
        /// </summary>
        public IRdfHandler Handler
        {
            get
            {
                return this._handler;
            }
        }

        /// <summary>
        /// Gets the Namespace Map for the parser context
        /// </summary>
        public INamespaceMapper Namespaces
        {
            get
            {
                return this._nsmap;
            }
        }

        /// <summary>
        /// Gets the Base URI for the parser context
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }
    }

    /// <summary>
    /// Class for Store Parser Contexts for Tokeniser based Parsing
    /// </summary>
    public class TokenisingStoreParserContext : BaseStoreParserContext
    {
        /// <summary>
        /// Tokeniser
        /// </summary>
        protected ITokenQueue _queue;
        /// <summary>
        /// Is Tokeniser traced?
        /// </summary>
        protected bool _traceTokeniser = false;
        /// <summary>
        /// Local Tokens
        /// </summary>
        protected Stack<IToken> _localTokens;

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with default settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser)
            : base(store)
        {
            this._queue = new TokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(store)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(store, tokeniser)
        {
            this._traceParsing = traceParsing;
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="store">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(ITripleStore store, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(store, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with default settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        public TokenisingStoreParserContext(IRdfHandler handler, ITokeniser tokeniser)
            : base(handler)
        {
            this._queue = new TokenQueue(tokeniser);
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        public TokenisingStoreParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode)
            : base(handler)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(IRdfHandler handler, ITokeniser tokeniser, bool traceParsing, bool traceTokeniser)
            : this(handler, tokeniser)
        {
            this._traceParsing = traceParsing;
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Creates a new Tokenising Store Parser Context with custom settings
        /// </summary>
        /// <param name="handler">Store to parse into</param>
        /// <param name="tokeniser">Tokeniser to use</param>
        /// <param name="queueMode">Tokeniser Queue Mode</param>
        /// <param name="traceParsing">Whether to trace parsing</param>
        /// <param name="traceTokeniser">Whether to trace tokenisation</param>
        public TokenisingStoreParserContext(IRdfHandler handler, ITokeniser tokeniser, TokenQueueMode queueMode, bool traceParsing, bool traceTokeniser)
            : base(handler, traceParsing)
        {
            switch (queueMode)
            {
                case TokenQueueMode.AsynchronousBufferDuringParsing:
                    this._queue = new AsynchronousBufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.SynchronousBufferDuringParsing:
                    this._queue = new BufferedTokenQueue(tokeniser);
                    break;
                case TokenQueueMode.QueueAllBeforeParsing:
                default:
                    this._queue = new TokenQueue(tokeniser);
                    break;
            }
            this._traceTokeniser = traceTokeniser;
            this._queue.Tracing = this._traceTokeniser;
        }

        /// <summary>
        /// Gets the Token Queue
        /// </summary>
        public ITokenQueue Tokens
        {
            get
            {
                return this._queue;
            }
        }

        /// <summary>
        /// Gets the Local Tokens stack
        /// </summary>
        public Stack<IToken> LocalTokens
        {
            get
            {
                if (this._localTokens == null) this._localTokens = new Stack<IToken>();
                return this._localTokens;
            }
        }

        /// <summary>
        /// Gets/Sets whether tokeniser tracing is used
        /// </summary>
        public bool TraceTokeniser
        {
            get
            {
                return this._traceTokeniser;
            }
            set
            {
                this._traceTokeniser = value;
            }
        }
    }
}
