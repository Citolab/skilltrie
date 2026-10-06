/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { BadgeInfo } from "@/types/badge-admin.ts";
import { useCallback, useEffect, useState } from "react";
import { FetchBadges, ModifyBadgeState, PublishBadge, RemoveBadge } from "@/api/badge-admin.ts";
import { toast } from "react-toastify";
import { BadgeCreator } from "@/components/badges/badge-creator";
import { PopupLayout } from "@/components/general-components/popup-layout.tsx";
import { BadgeCard } from "@/components/badges/badge-card.tsx";

/**
 * The properties of a badge which can be edited while it is staging.
 */
export interface BadgeProps {
    openFrom: string;
    openUntil: string;
    flagKey?: string;
    flagVariant?: string;
}

/**
 * Admin "Badges" page. Hosts the badge creator on top, then lists existing badges
 * split into Staging (editable, with publish/cancel/save actions) and Published
 * (read-only) sections. Owns the badge list and refreshes it after any mutation.
 */
export function BadgeSettings() {
    const [badges, setBadges] = useState<BadgeInfo[]>([]);
    const stagingBadges = badges.filter((b) => b.phase === "Staging");
    const publishedBadges = badges.filter((b) => b.phase === "Published");
    const [badgeAction, setBadgeAction] = useState<{
        identifier: string;
        action: "remove" | "publish";
    } | null>(null);

    /** Re-fetches the full badge list; called on mount and after every mutation. */
    const refresh = useCallback(() => {
        void FetchBadges().then((b) => setBadges(b));
    }, []);

    useEffect(() => {
        refresh();
    }, [refresh]);

    /** Publishes a staging badge, then refreshes the list. */
    const handlePublish = (identifier: string) => {
        PublishBadge(identifier)
            .then(() => {
                toast.success("Badge published");
                refresh();
            })
            .catch(() => toast.error("Failed to publish badge"));
    };

    /** Removes (cancels) a staging badge, then refreshes the list. */
    const handleRemove = (identifier: string) => {
        RemoveBadge(identifier)
            .then(() => {
                toast.success("Badge removed");
                refresh();
            })
            .catch(() => toast.error("Failed to remove badge"));
    };

    /**
     * Saves the availability window of a staging badge.
     * @param identifier the identifier of the badge to save the dates from
     * @param props
     */
    const handleSave = (identifier: string, props: BadgeProps) => {
        const flagKey = props.flagKey?.trim();
        const flagVariant = props.flagVariant?.trim();

        ModifyBadgeState({
            identifier,
            openFrom: new Date(props.openFrom),
            openUntil: new Date(props.openUntil),
            flagKey: flagKey != "" ? flagKey : undefined,
            flagVariant: flagVariant != "" ? flagVariant : undefined,
        })
            .then(() => {
                toast.success("Badge properties saved");
                refresh();
            })
            .catch(() => toast.error("Failed to save badge properties"));
    };

    return (
        <div className="p-8 text-nav-text-LD">
            <h1 className="text-4xl font-title font-bold text-text-primary mb-8">Badges</h1>

            <BadgeCreator onCreated={refresh} />

            <Section title="Staging">
                {stagingBadges.length === 0 ? (
                    <EmptyState text="No badges are currently staging." />
                ) : (
                    <BadgeGrid>
                        {stagingBadges.map((b) => (
                            <BadgeCard
                                key={b.identifier}
                                badge={b}
                                staging
                                onPublish={() =>
                                    setBadgeAction({ identifier: b.identifier, action: "publish" })
                                }
                                onRemove={() =>
                                    setBadgeAction({ identifier: b.identifier, action: "remove" })
                                }
                                onSave={handleSave}
                            />
                        ))}
                    </BadgeGrid>
                )}
            </Section>

            <Section title="Published">
                {publishedBadges.length === 0 ? (
                    <EmptyState text="No badges have been published yet." />
                ) : (
                    <BadgeGrid>
                        {publishedBadges.map((b) => (
                            <BadgeCard key={b.identifier} badge={b} />
                        ))}
                    </BadgeGrid>
                )}
            </Section>

            <PublishPopup
                open={badgeAction?.action === "publish"}
                onRefuse={() => setBadgeAction(null)}
                onPublish={() => {
                    setBadgeAction(null);
                    if (badgeAction != null) handlePublish(badgeAction.identifier);
                }}
            />
            <RemovePopup
                open={badgeAction?.action === "remove"}
                onRefuse={() => setBadgeAction(null)}
                onRemove={() => {
                    setBadgeAction(null);
                    if (badgeAction != null) handleRemove(badgeAction.identifier);
                }}
            />
        </div>
    );
}

/** A titled page section with a heading and divider, wrapping a group of cards. */
const Section = ({ title, children }: { title: string; children: React.ReactNode }) => (
    <section className="mb-10">
        <h2 className="text-2xl font-title font-semibold text-text-primary mb-1">{title}</h2>
        <hr className="border-borderDefault mb-4" />
        {children}
    </section>
);

/** Placeholder text shown when a section has no badges. */
const EmptyState = ({ text }: { text: string }) => <p className="text-text-muted italic">{text}</p>;

/** Responsive grid layout for the badge cards (1 column, 2 from the `xl` breakpoint). */
const BadgeGrid = ({ children }: { children: React.ReactNode }) => (
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">{children}</div>
);

function PublishPopup(props: { open: boolean; onRefuse: () => void; onPublish: () => void }) {
    return (
        <PopupLayout
            open={props.open}
            title={"Publish Badge?"}
            headerClassName={"cyan"}
            onClose={props.onRefuse}
            primaryAction={{ label: "Yes, Publish", onClick: props.onPublish }}
            secondaryAction={{ label: "No" }}
        >
            Are you sure you want to publish this badge? After publishing it,
            <b>you will not be able to undo this action and/or modify the badge</b>
        </PopupLayout>
    );
}

function RemovePopup(props: { open: boolean; onRefuse: () => void; onRemove: () => void }) {
    return (
        <PopupLayout
            open={props.open}
            title={"Remove Badge?"}
            headerClassName={"red"}
            onClose={props.onRefuse}
            primaryAction={{ label: "Yes, Remove", onClick: props.onRemove }}
            secondaryAction={{ label: "No" }}
        >
            Are you sure you want to remove this badge?
        </PopupLayout>
    );
}
