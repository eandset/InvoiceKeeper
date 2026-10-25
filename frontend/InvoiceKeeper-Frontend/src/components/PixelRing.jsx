import { useEffect, useRef } from 'react';

/**
 * 2D пиксельный круг: статично стоит в правой части hero,
 * пиксели мягко мерцают (живой цифровой шум). Не следит за курсором.
 */
export default function PixelRing({ size = 242, cell = 6 }) {
  const canvasRef = useRef(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return undefined;

    const ctx = canvas.getContext('2d');
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = size * dpr;
    canvas.height = size * dpr;
    ctx.scale(dpr, dpr);

    const center = size / 2;
    const radius = size * 0.42;
    const cols = Math.ceil(size / cell);
    const rows = Math.ceil(size / cell);

    // Предгенерируем «зерно» — базовая яркость каждой ячейки внутри круга.
    const cells = [];
    for (let gy = 0; gy < rows; gy += 1) {
      for (let gx = 0; gx < cols; gx += 1) {
        const x = gx * cell + cell / 2;
        const y = gy * cell + cell / 2;
        const dx = x - center;
        const dy = y - center;
        const dist = Math.hypot(dx, dy);
        if (dist > radius) continue;

        const edge = 1 - dist / radius; // 1 в центре, 0 на краю
        const noise = Math.random();
        if (noise > 0.18 + edge * 0.82) continue;

        cells.push({
          x: gx * cell,
          y: gy * cell,
          edge,
          phase: Math.random() * Math.PI * 2,
          speed: 0.0009 + Math.random() * 0.0022,
          base: 0.12 + edge * 0.6,
          alt: Math.random() > 0.72
        });
      }
    }

    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    let raf = 0;
    let start = performance.now();

    function color() {
      const value = getComputedStyle(document.documentElement).getPropertyValue('--accent').trim();
      return value || '#ff3b3b';
    }

    function colorAlt() {
      const value = getComputedStyle(document.documentElement).getPropertyValue('--accent-2').trim();
      return value || '#ff6b6b';
    }

    function draw(now) {
      ctx.clearRect(0, 0, size, size);

      // Мягкое кольцо-ореол внутри canvas
      const gradient = ctx.createRadialGradient(center, center, radius * 0.1, center, center, radius * 1.02);
      gradient.addColorStop(0, 'rgba(255,255,255,0.05)');
      gradient.addColorStop(0.72, 'rgba(255,255,255,0.012)');
      gradient.addColorStop(1, 'rgba(255,255,255,0)');
      ctx.fillStyle = gradient;
      ctx.beginPath();
      ctx.arc(center, center, radius * 1.02, 0, Math.PI * 2);
      ctx.fill();

      const main = color();
      const alt = colorAlt();
      const t = now - start;

      for (const c of cells) {
        const wave = prefersReduced ? 0.5 : 0.5 + 0.5 * Math.sin(t * c.speed + c.phase);
        const alpha = Math.min(0.92, c.base + wave * (0.18 + c.edge * 0.34));
        ctx.globalAlpha = alpha;
        ctx.fillStyle = c.alt ? alt : main;
        ctx.fillRect(c.x, c.y, cell - 0.6, cell - 0.6);
      }

      ctx.globalAlpha = 1;

      // Тонкая пиксельная орбита по краю
      ctx.strokeStyle = main;
      ctx.globalAlpha = 0.16;
      ctx.lineWidth = 1;
      ctx.beginPath();
      ctx.arc(center, center, radius + cell * 0.9, 0, Math.PI * 2);
      ctx.stroke();
      ctx.globalAlpha = 1;

      if (!prefersReduced) raf = requestAnimationFrame(draw);
    }

    draw(performance.now());

    const onVisibility = () => {
      if (document.hidden) {
        cancelAnimationFrame(raf);
      } else if (!prefersReduced) {
        start = performance.now();
        raf = requestAnimationFrame(draw);
      }
    };

    document.addEventListener('visibilitychange', onVisibility);

    return () => {
      cancelAnimationFrame(raf);
      document.removeEventListener('visibilitychange', onVisibility);
    };
  }, [size, cell]);

  return (
    <div className="pixel-ring" style={{ width: size, height: size }} aria-hidden="true">
      <span className="pixel-ring__halo" />
      <canvas ref={canvasRef} />
    </div>
  );
}
