/**
 * Memetoğlu Taşımacılık — site betiği.
 *
 * Üç iş yapar:
 *   1. Kaydırma animasyonlarını tetikler
 *   2. Menüyü saydamdan beyaza çevirir ve mobil menüyü açar/kapatır
 *   3. Hero videosunu, yalnızca gerektiğinde ve uygun cihazlarda yükler
 *
 * JavaScript kapalıysa site tüm içeriğiyle normal biçimde görünür.
 */
(function () {
  'use strict';

  var kok = document.documentElement;
  kok.classList.add('js');

  var azHareket =
    window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  /* ----------------------------------------------------------------
     1) Kaydırma animasyonları
     ---------------------------------------------------------------- */
  var ogeler = document.querySelectorAll('[data-reveal]');

  if (!('IntersectionObserver' in window)) {
    // Eski tarayıcı: her şeyi doğrudan göster.
    Array.prototype.forEach.call(ogeler, function (el) {
      el.setAttribute('data-visible', 'true');
    });
  } else {
    var gozlemci = new IntersectionObserver(
      function (girisler) {
        girisler.forEach(function (giris) {
          if (giris.isIntersecting) {
            giris.target.setAttribute('data-visible', 'true');
            gozlemci.unobserve(giris.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: '0px 0px -8% 0px' }
    );
    Array.prototype.forEach.call(ogeler, function (el) {
      gozlemci.observe(el);
    });
  }

  /* ----------------------------------------------------------------
     2) Menü
     ---------------------------------------------------------------- */
  var bas = document.getElementById('site-head');
  var dugme = document.getElementById('menu-toggle');
  var menu = document.getElementById('mobil-menu');
  var menuAcik = false;

  function basGuncelle() {
    if (!bas) return;
    // Menü açıkken her zaman katı zemin: saydam menünün üstünde
    // beyaz panel garip durur.
    var katiOlmali = menuAcik || window.scrollY > 40;
    bas.classList.toggle('is-solid', katiOlmali);
  }

  function menuAyarla(acik) {
    menuAcik = acik;
    if (!dugme || !menu) return;
    dugme.setAttribute('aria-expanded', acik ? 'true' : 'false');
    dugme.setAttribute('aria-label', acik ? 'Menüyü kapat' : 'Menüyü aç');
    menu.hidden = !acik;
    document.body.style.overflow = acik ? 'hidden' : '';
    basGuncelle();
  }

  if (dugme && menu) {
    dugme.addEventListener('click', function () {
      menuAyarla(dugme.getAttribute('aria-expanded') !== 'true');
    });
    Array.prototype.forEach.call(menu.querySelectorAll('a'), function (a) {
      a.addEventListener('click', function () {
        menuAyarla(false);
      });
    });
    window.addEventListener('keydown', function (olay) {
      if (olay.key === 'Escape' && menuAcik) menuAyarla(false);
    });
  }

  var bekliyor = false;
  window.addEventListener(
    'scroll',
    function () {
      if (bekliyor) return;
      bekliyor = true;
      window.requestAnimationFrame(function () {
        basGuncelle();
        bekliyor = false;
      });
    },
    { passive: true }
  );
  basGuncelle();

  /* ----------------------------------------------------------------
     3) Hero videosu
     Poster her zaman görünür; video yalnızca aşağıdaki koşullar
     sağlanırsa indirilir. Böylece mobil veri ve açılış hızı korunur.
     ---------------------------------------------------------------- */
  var video = document.getElementById('hero-video');

  if (video) {
    var dar = window.matchMedia('(max-width: 860px)').matches;
    var veriTasarrufu =
      navigator.connection &&
      (navigator.connection.saveData ||
        /2g/.test(navigator.connection.effectiveType || ''));

    if (!dar && !azHareket && !veriTasarrufu) {
      var webm = video.getAttribute('data-webm');
      var mp4 = video.getAttribute('data-mp4');

      // Kaynakları sırayla ekle: tarayıcı desteklediği ilkini seçer.
      // WebM önce gelir (daha küçük dosya), MP4 Safari için yedektir.
      [
        [webm, 'video/webm'],
        [mp4, 'video/mp4'],
      ].forEach(function (cift) {
        if (!cift[0]) return;
        var kaynak = document.createElement('source');
        kaynak.src = cift[0];
        kaynak.type = cift[1];
        video.appendChild(kaynak);
      });

      if (video.firstChild) {
        video.addEventListener(
          'canplay',
          function () {
            video.classList.add('is-ready');
            var oynat = video.play();
            if (oynat && typeof oynat.catch === 'function') {
              // Tarayıcı oynatmayı reddederse poster görünmeye devam eder.
              oynat.catch(function () {
                video.classList.remove('is-ready');
              });
            }
          },
          { once: true }
        );
        // Hiçbir biçim oynatılamazsa sessizce poster'da kal.
        video.addEventListener('error', function () {
          video.classList.remove('is-ready');
        });
        video.load();
      }
    }

    // Sekme arka plandayken boşuna çalışmasın.
    document.addEventListener('visibilitychange', function () {
      if (!video.currentSrc) return;
      if (document.hidden) {
        video.pause();
      } else if (video.classList.contains('is-ready')) {
        var o = video.play();
        if (o && typeof o.catch === 'function') o.catch(function () {});
      }
    });
  }

  /* ----------------------------------------------------------------
     4) Form sonucunu ekranda göster
     gonder.php ?sonuc=... ile geri döner; kullanıcıyı forma kaydırır.
     ---------------------------------------------------------------- */
  var sonuc = document.getElementById('form-sonuc');
  if (sonuc) {
    sonuc.scrollIntoView({ behavior: azHareket ? 'auto' : 'smooth', block: 'center' });
  }
})();
