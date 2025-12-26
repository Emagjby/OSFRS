import {
    computePosition,
    offset,
    flip,
    shift,
    arrow as arrowMw,
    autoUpdate,
    type Placement,
} from "@floating-ui/dom";

export function tooltip(
    node: HTMLElement,
    { label, placement = "right" }: { label: string; placement?: Placement },
) {
    let floatingEl: HTMLDivElement | null = null;
    let arrowEl: HTMLDivElement | null = null;
    let cleanup: (() => void) | null = null;

    function show() {
        if (floatingEl) return;

        floatingEl = document.createElement("div");
        floatingEl.className = "tooltip";
        floatingEl.textContent = label;

        arrowEl = document.createElement("div");
        arrowEl.className = "tooltip-arrow";
        floatingEl.appendChild(arrowEl);

        document.getElementById("tooltip-root")?.appendChild(floatingEl);

        cleanup = autoUpdate(node, floatingEl, async () => {
            const {
                x,
                y,
                middlewareData,
                placement: p,
            } = await computePosition(node, floatingEl!, {
                placement,
                middleware: [
                    offset(16),
                    flip(),
                    shift({ padding: 8 }),
                    arrowMw({ element: arrowEl! }),
                ],
            });

            Object.assign(floatingEl!.style, {
                left: `${x}px`,
                top: `${y}px`,
            });

            const arrowData = middlewareData.arrow as
                | { x?: number; y?: number }
                | undefined;
            if (!arrowData || !arrowEl) return;

            const side = p.split("-")[0];

            arrowEl.style.left = "";
            arrowEl.style.top = "";
            arrowEl.style.right = "";
            arrowEl.style.bottom = "";

            if (arrowData.x != null) {
                arrowEl.style.left = `${arrowData.x}px`;
            }
            if (arrowData.y != null) {
                arrowEl.style.top = `${arrowData.y}px`;
            }

            const staticSide = {
                top: "bottom",
                right: "left",
                bottom: "top",
                left: "right",
            } as const;

            const sideProp = staticSide[side as keyof typeof staticSide];

            arrowEl.style[sideProp] = "-5px";
        });
    }

    function hide() {
        cleanup?.();
        cleanup = null;
        floatingEl?.remove();
        floatingEl = null;
    }

    node.addEventListener("mouseenter", show);
    node.addEventListener("mouseleave", hide);
    node.addEventListener("focus", show);
    node.addEventListener("blur", hide);

    return {
        destroy() {
            hide();
            node.removeEventListener("mouseenter", show);
            node.removeEventListener("mouseleave", hide);
            node.removeEventListener("focus", show);
            node.removeEventListener("blur", hide);
        },
    };
}
