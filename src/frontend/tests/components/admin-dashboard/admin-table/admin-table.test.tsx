/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminTable from "../../../../src/components/admin-dashboard/admin-table/admin-table";

describe("Headers of table", () => {
    function renderHeaderTable() {
        render(
            <AdminTable
                rows={[
                    { id: 1, name: "Hans", email: "hans@example.com", active: true },
                    { id: 2, name: "Conrad", email: "conrad@example.com", active: false },
                ]}
                objectKeys={["id", "name", "email", "active"]}
                optionalKeys={["email"]}
                headerNames={{ name: "Person", active: "Status" }}
            />
        );
    }

    test("Keys without names present", () => {
        renderHeaderTable();
        expect(screen.getByText(/id/i)).toBeInTheDocument();
        expect(screen.getByText(/email/i)).toBeInTheDocument();
    });

    test("Headernames present", () => {
        renderHeaderTable();
        expect(screen.getByText(/Person/i)).toBeInTheDocument();
        expect(screen.getByText(/Status/i)).toBeInTheDocument();
    });

    test("Keys with headernames absent", () => {
        renderHeaderTable();
        expect(screen.queryByText(/name/i)).not.toBeInTheDocument();
        expect(screen.queryByText(/active/i)).not.toBeInTheDocument();
    });

    test("Optional headers and column hiddens on smaller screen", () => {
        renderHeaderTable();
        expect(screen.queryByText(/email/i)).toHaveClass("hidden");
        expect(screen.queryByText(/hans@example.com/i)).toHaveClass("hidden");
    });
});

describe("Column data", () => {
    function renderHeaderTable() {
        render(
            <AdminTable
                rows={[
                    { id: 1, name: "Hans", email: "hans@example.com", active: true },
                    { id: 2, name: "Conrad", email: "conrad@example.com", active: false },
                ]}
                objectKeys={["id", "name", "email", "active"]}
                optionalKeys={["email"]}
                headerNames={{ name: "Person" }}
                transformColumnData={{
                    active: (cell) => (cell ? "🟢 Activated" : "🔴 Deactivated"),
                }}
            />
        );
    }
    test("Untransformed data rendered", () => {
        renderHeaderTable();
        expect(screen.getByText(/1/i)).toBeInTheDocument();
        expect(screen.getByText(/^Hans$/i)).toBeInTheDocument();
        expect(screen.getByText(/^hans@example.com$/i)).toBeInTheDocument();

        expect(screen.getByText(/2/i)).toBeInTheDocument();
        expect(screen.getByText(/^Conrad$/i)).toBeInTheDocument();
        expect(screen.getByText(/^conrad@example.com$/i)).toBeInTheDocument();
    });

    test("Transformed data rendered", () => {
        renderHeaderTable();
        expect(screen.getByText(/🟢 Activated/i)).toBeInTheDocument();
        expect(screen.getByText(/🔴 Deactivated/i)).toBeInTheDocument();
    });

    test("Transformed column data without original data", () => {
        renderHeaderTable();
        expect(screen.queryByText(/true/i)).not.toBeInTheDocument();
        expect(screen.queryByText(/false/i)).not.toBeInTheDocument();
    });
});
describe("TableActions", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    const onDelete = vi.fn();
    const onRowClick = vi.fn();
    function renderHeaderTable() {
        render(
            <AdminTable
                rows={[
                    { id: 1, name: "Hans", active: true },
                    { id: 2, name: "Conrad", active: false },
                ]}
                objectKeys={["id", "name", "active"]}
                tableActions={{
                    delete: {
                        action: onDelete,
                    },
                }}
                onRowClick={onRowClick}
                renderExpandedRow={(row, index) => {
                    return (
                        <div>
                            Hello {row.name} on {index}
                        </div>
                    );
                }}
            />
        );
    }

    test("Action click calls row", () => {
        renderHeaderTable();

        const deleteButtons = screen.getAllByRole("button", { name: /delete/i });
        fireEvent.click(deleteButtons[1]);

        expect(onDelete).toHaveBeenCalledWith({
            active: false,
            id: 2,
            name: "Conrad",
        });
    });

    test("Row click calls with row data and index", () => {
        renderHeaderTable();
        const renderedRows = screen.getAllByRole("row").slice(1);
        fireEvent.click(renderedRows[1]);

        expect(onRowClick).toHaveBeenCalledWith({ id: 2, name: "Conrad", active: false }, 1);
    });

    test("Expanded row click shows node", () => {
        renderHeaderTable();
        const renderedRows = screen.getAllByRole("row").slice(1);
        expect(screen.queryByText(/Hello/)).not.toBeInTheDocument();
        fireEvent.click(renderedRows[1]);
        expect(screen.queryByText(/Hello Conrad on 1/)).toBeInTheDocument();
    });
});
