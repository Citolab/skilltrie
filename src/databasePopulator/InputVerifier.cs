/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace DatabasePopulator;

internal class InputVerifier
{
    // verify functions, each function check one constraint not accounted for in InputParser
    // constraints should be regarding whether the database as a whole is valid
    private delegate bool VerifyFunction(AppDbContext context);

    // general verify function
    // calls all other verify functions, returns bool denoting whether the database is valid and the corresponding error
    public async Task Verify(AppDbContext context)
    {
        Program.Status("\nStarting checking constraints...");

        VerifyFunction[] verifyFunctions =
        {
            VerifyEdgelistAcyclicity,
            VerifyValidScopeMemberships,
            VerifyNoRepeatDomains
        };
        int count = 0;

        foreach (VerifyFunction verifyFunction in verifyFunctions)
            if (verifyFunction(context))
                count++;

        Program.Success($"\nConstraints successfully checked: {count}/{verifyFunctions.Length} constraints passed\n");
    }

    // The edgelist (knowledge tree as seen on the home page) has to be acyclic
    private bool VerifyEdgelistAcyclicity(AppDbContext context)
    {
        Program.Status("\nChecking for edgelist acyclicity...");

        bool result = true;

        try
        {
            foreach (Scope scope in context.Scopes.ToList())
            {
                ICollection<int> passedFromScopes = [],
                    passedToScopes = [];

                CheckAcycliclicity(scope, passedFromScopes, scope => Program.GetFromScopes(context, scope).ToList());
                CheckAcycliclicity(scope, passedToScopes, scope => Program.GetToScopes(context, scope).ToList());
            }
        }
        catch (Exception exception)
        {
            Program.Error(exception);

            result = false;
        }

        Program.Success($"Successfully checked for edgelist acyclicity: {(result ? "passed" : "fail")}");

        return result;
    }

    // In the application we make certain assumptions regarding the scope memberships
    private bool VerifyValidScopeMemberships(AppDbContext context)
    {
        Program.Status("\nChecking for valid scope memberships...");

        bool result = true;

        foreach (Scope scope in context.Scopes.ToList())
        {
            IEnumerable<Scope> ancestors = Program.GetAncestors(context, scope);
            IEnumerable<Scope> descendants = Program.GetDescendants(context, scope);

            try
            {
                if (scope.Type == ScopeType.Subject)
                {
                    // subjects don't have ancestors
                    if (ancestors.Count() > 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a subject with ancestors");

                    // subjects have descendants
                    if (descendants.Count() == 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a subject without descendants");

                    // topics only have domains as descendant
                    foreach (Scope descendant in descendants)
                        if (descendant.Type != ScopeType.Domain)
                            throw new Exception($"Scope.Name='{scope.Name}' is a subject with non-domain descendant Scope.Name='{descendant.Name}'");
                }
                else if (scope.Type == ScopeType.Domain)
                {
                    // domains have ancestors
                    if (ancestors.Count() == 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a domain without ancestors");

                    // domains have descendants
                    if (descendants.Count() == 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a domain without descendants");

                    // check for acyclicity
                    ICollection<int> passedScopes = [];

                    CheckAcycliclicity(scope, passedScopes, scope => Program.GetAncestors(context, scope).ToList());
                    CheckAcycliclicity(scope, passedScopes, scope => Program.GetDescendants(context, scope).ToList());
                }
                else if (scope.Type == ScopeType.Topic)
                {
                    // topics have an ancestor
                    if (ancestors.Count() == 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a topic without ancestors");

                    // topics have an ancestor
                    if (ancestors.Count() > 1)
                        throw new Exception($"Scope.Name='{scope.Name}' is a topic with multiple ancestors");

                    // topics don't have descendants
                    if (descendants.Count() > 0)
                        throw new Exception($"Scope.Name='{scope.Name}' is a topic with descendants");

                    // topics only have domains as ancestor
                    foreach (Scope ancestor in ancestors)
                        if (ancestor.Type != ScopeType.Domain)
                            throw new Exception($"Scope.Name='{scope.Name}' is a topic with non-domain ancestor Scope.Name='{ancestor.Name}'");
                }
            }
            catch (Exception exception)
            {
                Program.Error(exception);

                result = false;
            }
        }

        Program.Success($"Successfully checked for valid scope memberships: {(result ? "passed" : "fail")}");

        return result;
    }

    // the scope memberships (i.e. what scope does a scope belong to) has to be acyclic
    private bool VerifyNoRepeatDomains(AppDbContext context)
    {
        Program.Status("\nChecking for repeating domains...");

        bool result = true;

        foreach (Scope scope in context.Scopes.Include(s => s.Descendants).ToList())
            if (scope.Type == ScopeType.Subject)
                try
                {
                    if (!CheckSubjectNoRepeatDomains(context, scope))
                        throw new Exception($"Scope.Name='{scope.Name}' is a subject with repeating domains");
                }
                catch (Exception exception)
                {
                    Program.Error(exception);

                    result = false;
                }

        Program.Success($"Successfully checked for repeating domains: {(result ? "passed" : "fail")}");

        return result;
    }

    // Recursive helper function to check for acyclicity
    private void CheckAcycliclicity(Scope currentScope, ICollection<int> passedScopes, Func<Scope, IEnumerable<Scope>> nextScopes)
    {
        passedScopes.Add(currentScope.Id);

        foreach (Scope nextScope in nextScopes(currentScope))
            if (passedScopes.Contains(nextScope.Id))
                throw new Exception($"Scope.Id='{nextScope.Id}' is part of the cycle");
            else
                CheckAcycliclicity(nextScope, new List<int>(passedScopes), nextScopes);
    }

    // Helper function for InputVerifier.VerifyNoRepeatDomains
    // same function as InputVerifier.VerifyNoRepeatDomains, but for specific subject
    private bool CheckSubjectNoRepeatDomains(AppDbContext context, Scope subject)
    {
        bool result = true;

        IEnumerable<Scope> descendants = Program.GetDescendants(context, subject);

        foreach (Scope descendant in descendants)
        {
            // Not implemented exception
            throw new Exception("Parser.CheckSubjectNoRepeatDomains: Not implemented");
        }

        return result;
    }
}
