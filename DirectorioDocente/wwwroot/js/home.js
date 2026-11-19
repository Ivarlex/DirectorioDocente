document.addEventListener("DOMContentLoaded", () => {

    const tl = gsap.timeline({ defaults: { ease: "power3.out" } });

    tl.from('[data-anim="badge"]', { y: 20, opacity: 0, duration: 0.5 })
        .from('[data-anim="title"]', { y: 40, opacity: 0, duration: 0.8 }, "-=0.2")
        .from('[data-anim="subtitle"]', { y: 30, opacity: 0, duration: 0.7 }, "-=0.4")
        .from('[data-anim="cta"]', { y: 20, opacity: 0, duration: 0.6 }, "-=0.3")
        .from('[data-anim="card"]', {
            y: 30,
            opacity: 0,
            duration: 0.6,
            stagger: 0.15
        }, "-=0.2");

    gsap.to(".blob", {
        x: "random(-40, 40)",
        y: "random(-40, 40)",
        scale: "random(0.9, 1.15)",
        duration: 6,
        ease: "sine.inOut",
        repeat: -1,
        yoyo: true,
        stagger: {
            each: 1.5,
            from: "random"
        }
    });

    document.querySelectorAll(".feature-card").forEach((card) => {
        card.addEventListener("mouseenter", () => {
            gsap.to(card, { y: -8, boxShadow: "0 20px 25px -5px rgb(0 0 0 / 0.1)", duration: 0.3 });
        });
        card.addEventListener("mouseleave", () => {
            gsap.to(card, { y: 0, boxShadow: "0 1px 2px 0 rgb(0 0 0 / 0.05)", duration: 0.3 });
        });
    });

    document.querySelectorAll(".btn-primary").forEach((btn) => {
        btn.addEventListener("mouseenter", () => gsap.to(btn, { scale: 1.03, duration: 0.2 }));
        btn.addEventListener("mouseleave", () => gsap.to(btn, { scale: 1, duration: 0.2 }));
    });
});