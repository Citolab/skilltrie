/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ComputedContext, ComputedItem } from "@citolab/qti-components";

class ComputedContextHelper {
    private readonly computedContext: ComputedContext | null;

    constructor(computedContext: ComputedContext | null) {
        // eslint-disable-next-line no-restricted-syntax
        this.computedContext = computedContext;
    }

    get contextReady(): boolean {
        return this.computedContext !== null;
    }

    get activeIndex(): number {
        return this.activeItem.index ?? -1;
    }

    get activeItem(): ComputedItem {
        // prettier-ignore
        const activeItem: ComputedItem | undefined =
            this.computedContext
                ?.testParts.find(t => t.active)
                ?.sections.find(s => s.active)
                ?.items.find(i => i.active);

        if (!activeItem) {
            console.error("computed context:", this.computedContext);
            throw new Error(`No active item, has the QTI player finished loading?`);
        }

        return activeItem;
    }

    get activeSectionItems(): ComputedItem[] {
        // prettier-ignore
        const activeSectionItems: ComputedItem[] | undefined =
            this.computedContext
                ?.testParts.find(t => t.active)
                ?.sections.find(s => s.active)
                ?.items

        if (!activeSectionItems) {
            console.error("computed context:", this.computedContext);
            throw new Error(`No active section, has the QTI player finished loading?`);
        }

        return activeSectionItems;
    }

    get allItemsCompleted(): boolean {
        return this.activeSectionItems.every((i) => i.completionStatus === "completed");
    }

    get activeItemCompleted(): boolean {
        return this.activeItem.completionStatus === "completed";
    }

    get isLastItem(): boolean {
        // prettier-ignore
        return (
            this.activeItem.index
            ===
            this.activeSectionItems.length
        );
    }

    itemByIndex(index: number): ComputedItem {
        return this.activeSectionItems[index % this.activeSectionItems.length];
    }

    itemIdentifierByIndex(index: number): string {
        return this.itemByIndex(index).identifier;
    }
}

export default ComputedContextHelper;
