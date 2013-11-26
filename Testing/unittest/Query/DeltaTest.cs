﻿/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.IO;
using System.Linq;
using NUnit.Framework;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class DeltaTest
    {
        private const string TestData = @"
<http://r1> <http://r1> <http://r1> .
<http://r2> <http://r2> <http://r2> .
";

        private const string MinusQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    MINUS
    {
        GRAPH <http://g0> { ?s ?p ?o }
    }
}
";

        private const string OptionalSameTermQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://g0> { ?s ?p ?o0 . }
        FILTER (SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?o0))
}
";

        private const string OptionalInvertedSameTermQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://g0> { ?s ?p ?o0 . }
    }
    FILTER (!SAMETERM(?o, ?o0))
}
";
        private const string NotExistsQuery = @"
SELECT *
WHERE
{
    GRAPH <http://g1>
    {
        ?s ?p ?o .
    }
    FILTER NOT EXISTS { GRAPH <http://g0> { ?s ?p ?o } }
}
";

        private void TestQuery(IInMemoryQueryableStore store, String query, String queryName, int differences)
        {
            using (SparqlResultSet resultSet = (SparqlResultSet)store.ExecuteQuery(query))
            {
                Console.WriteLine(queryName + ": " + resultSet.Count);
                foreach (SparqlResult result in resultSet)
                {
                    Console.WriteLine(result.ToString());
                }
                Assert.AreEqual(differences, resultSet.Count);
            }
            Console.WriteLine();
        }

        private void TestDeltas(IGraph a, IGraph b, int differences)
        {
            a.BaseUri = new Uri("http://g1");
            b.BaseUri = new Uri("http://g0");

            IInMemoryQueryableStore store = new TripleStore();
            store.Add(a);
            store.Add(b);

            this.TestQuery(store, MinusQuery, "Minus", differences);
            this.TestQuery(store, OptionalSameTermQuery, "OptionalSameTerm", differences);
            this.TestQuery(store, OptionalInvertedSameTermQuery, "OptionalInvertedSameTerm", differences);
            this.TestQuery(store, NotExistsQuery, "NotExists", differences);
        }

        [Test]
        public void SparqlGraphDeltas1()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            new TurtleParser().Load(a, new StringReader(TestData));
            new TurtleParser().Load(b, new StringReader(TestData));

            this.TestDeltas(a, b, 0);
        }

        [Test]
        public void SparqlGraphDeltas2()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            new TurtleParser().Load(a, new StringReader(TestData));
            new TurtleParser().Load(b, new StringReader(TestData));
            b.Retract(b.GetTriplesWithSubject(new Uri("http://r1")).ToList());

            this.TestDeltas(a, b, 1);
        }

        [Test]
        public void SparqlGraphDeltas3()
        {
            IGraph a = new Graph();
            IGraph b = new Graph();

            new TurtleParser().Load(a, new StringReader(TestData));

            this.TestDeltas(a, b, 2);
        }
    }
}