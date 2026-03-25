/**
 * STEFANO RICCI — Premium SaaS Interaction Engine v3.0
 * Handles: Page loader, sidebar mobile, theme, animated counters,
 *          3D card tilt, button ripple, toast system, stagger animations,
 *          progress bar animation, keyboard shortcuts.
 */
(function () {
	'use strict';

	/* ==========================================================
	   1. PAGE LOADING INDICATOR
	========================================================== */
	window.addEventListener('load', function () {
		var loader = document.getElementById('page-loader');
		if (loader) {
			loader.classList.add('loaded');
			setTimeout(function () { loader.remove(); }, 650);
		}
	});

	document.addEventListener('click', function (e) {
		var link = e.target.closest('a[href]');
		if (!link) return;
		var href = link.getAttribute('href');
		if (!href || href.startsWith('#') || href.startsWith('javascript') || link.getAttribute('target') === '_blank') return;
		var loader = document.getElementById('page-loader');
		if (loader) loader.classList.remove('loaded');
	});

	/* ==========================================================
	   2. MOBILE SIDEBAR TOGGLE
	========================================================== */
	var sidebar = document.querySelector('.app-sidebar');
	var hamburger = document.getElementById('hamburgerBtn');
	var overlay = document.getElementById('sidebar-overlay');

	function openSidebar() {
		if (sidebar) sidebar.classList.add('open');
		if (overlay) overlay.classList.add('active');
		document.body.style.overflow = 'hidden';
	}

	function closeSidebar() {
		if (sidebar) sidebar.classList.remove('open');
		if (overlay) overlay.classList.remove('active');
		document.body.style.overflow = '';
	}

	function toggleSidebar() {
		if (window.innerWidth < 992) {
			if (sidebar && sidebar.classList.contains('open')) {
				sidebar.classList.remove('open');
				if (overlay) overlay.classList.remove('active');
				document.body.style.overflow = '';
			} else {
				if (sidebar) sidebar.classList.add('open');
				if (overlay) overlay.classList.add('active');
				document.body.style.overflow = 'hidden';
			}
		} else {
			var appLayout = document.querySelector('.app-layout');
			if (appLayout) {
				appLayout.classList.toggle('sidebar-collapsed');
				var isCollapsed = appLayout.classList.contains('sidebar-collapsed');
				localStorage.setItem('sr-sidebar-collapsed', isCollapsed ? 'true' : 'false');
			}
		}
	}

	var storedLayout = document.querySelector('.app-layout');
	if (localStorage.getItem('sr-sidebar-collapsed') === 'true' && storedLayout && window.innerWidth >= 992) {
		storedLayout.classList.add('sidebar-collapsed');
	}

	if (hamburger) hamburger.addEventListener('click', toggleSidebar);
	if (overlay) overlay.addEventListener('click', closeSidebar);

	/* ==========================================================
	   3. ACTIVE SIDEBAR LINK
	========================================================== */
	var currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
	document.querySelectorAll('.sidebar-link').forEach(function (link) {
		var href = (link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '');
		if (!href) return;
		var isHome = (href === '/home/index' || href === '/') && (currentPath === '/' || currentPath === '/home/index' || currentPath === '/home');
		var isActive = isHome || (href !== '/' && href !== '/home/index' && currentPath.startsWith(href.split('/index')[0]));
		if (isActive) link.classList.add('active');
	});

	/* ==========================================================
	   4. THEME ENGINE
	========================================================== */
	var lightTheme = document.getElementById('dx-theme-light');
	var darkTheme = document.getElementById('dx-theme-dark');
	var themeIcon = document.getElementById('themeIcon');
	var themeToggleBtn = document.getElementById('themeToggleBtn');
	var isDarkMode = true;

	function setTheme(isDark) {
		isDarkMode = isDark;
		if (isDark) {
			if (lightTheme) lightTheme.disabled = true;
			if (darkTheme) darkTheme.disabled = false;
			document.body.classList.add('theme-dark');
			document.body.classList.remove('theme-light');
			if (themeIcon) themeIcon.className = 'bi bi-moon-stars-fill';
			localStorage.setItem('sr-theme', 'dark');
		} else {
			if (lightTheme) lightTheme.disabled = false;
			if (darkTheme) darkTheme.disabled = true;
			document.body.classList.add('theme-light');
			document.body.classList.remove('theme-dark');
			if (themeIcon) themeIcon.className = 'bi bi-sun-fill';
			localStorage.setItem('sr-theme', 'light');
		}
	}

	if (themeToggleBtn) {
		themeToggleBtn.addEventListener('click', function () { setTheme(!isDarkMode); });
	}

	var savedTheme = localStorage.getItem('sr-theme');
	setTheme(savedTheme !== 'light');

	/* ==========================================================
	   5. ANIMATED COUNTERS (DASHBOARD STATS)
	========================================================== */
	function animateCounter(el) {
		var raw = el.textContent.replace(/[^\d]/g, '');
		var target = parseInt(raw, 10);
		if (isNaN(target) || target === 0) return;
		el.setAttribute('data-target', target);
		var duration = 1500;
		var startTime = null;

		function step(ts) {
			if (!startTime) startTime = ts;
			var progress = Math.min((ts - startTime) / duration, 1);
			var eased = 1 - Math.pow(1 - progress, 3);
			el.textContent = Math.round(eased * target);
			if (progress < 1) requestAnimationFrame(step);
			else el.textContent = target;
		}
		requestAnimationFrame(step);
	}

	if ('IntersectionObserver' in window) {
		var counterObserver = new IntersectionObserver(function (entries) {
			entries.forEach(function (entry) {
				if (entry.isIntersecting) {
					animateCounter(entry.target);
					counterObserver.unobserve(entry.target);
				}
			});
		}, { threshold: 0.5 });

		document.querySelectorAll('.display-5').forEach(function (el) {
			counterObserver.observe(el);
		});
	}

	/* ==========================================================
	   6. BUTTON RIPPLE EFFECT
	========================================================== */
	document.addEventListener('click', function (e) {
		var btn = e.target.closest('.btn');
		if (!btn || btn.disabled) return;
		var existing = btn.querySelector('.sr-ripple');
		if (existing) existing.remove();
		var rect = btn.getBoundingClientRect();
		var size = Math.max(rect.width, rect.height) * 2.2;
		var ripple = document.createElement('span');
		ripple.className = 'sr-ripple';
		ripple.style.cssText = [
			'position:absolute',
			'border-radius:50%',
			'pointer-events:none',
			'transform:scale(0)',
			'animation:sr-ripple-anim 0.58s linear',
			'background:rgba(255,255,255,0.18)',
			'width:' + size + 'px',
			'height:' + size + 'px',
			'left:' + (e.clientX - rect.left - size / 2) + 'px',
			'top:' + (e.clientY - rect.top - size / 2) + 'px',
			'z-index:0'
		].join(';');
		btn.appendChild(ripple);
		ripple.addEventListener('animationend', function () { ripple.remove(); });
	});

	/* ==========================================================
	   7. TOAST NOTIFICATION SYSTEM
	========================================================== */
	window.SRToast = {
		_container: null,
		_getContainer: function () {
			if (!this._container) {
				this._container = document.createElement('div');
				this._container.id = 'sr-toast-container';
				document.body.appendChild(this._container);
			}
			return this._container;
		},
		show: function (message, type) {
			type = type || 'info';
			var icons = { success: 'bi-check-circle-fill', danger: 'bi-exclamation-triangle-fill', info: 'bi-info-circle-fill', warning: 'bi-exclamation-circle-fill' };
			var colors = { success: '#1B9A70', danger: '#C94E28', info: '#2F6EC4', warning: '#C4921E' };
			var toast = document.createElement('div');
			toast.className = 'sr-toast sr-toast--' + type;
			toast.innerHTML = '<i class="bi ' + (icons[type] || icons.info) + '" style="color:' + (colors[type] || colors.info) + ';font-size:1rem;flex-shrink:0;"></i><span>' + message + '</span>';
			this._getContainer().appendChild(toast);
			var self = this;
			setTimeout(function () {
				toast.classList.add('sr-toast--out');
				setTimeout(function () { toast.remove(); }, 380);
			}, 3800);
		}
	};

	/* ==========================================================
	   9. STAGGERED ENTRANCE ANIMATIONS
	========================================================== */
	if ('IntersectionObserver' in window) {
		var staggerObserver = new IntersectionObserver(function (entries) {
			entries.forEach(function (entry) {
				if (entry.isIntersecting) {
					entry.target.classList.add('stagger-visible');
					staggerObserver.unobserve(entry.target);
				}
			});
		}, { threshold: 0.06 });

		document.querySelectorAll('.row > [class*="col-"]').forEach(function (el, i) {
			el.style.setProperty('--stagger-i', String(i));
			el.classList.add('stagger-item');
			staggerObserver.observe(el);
		});
	}

	/* ==========================================================
	   10. PROGRESS BAR ANIMATION
	========================================================== */
	if ('IntersectionObserver' in window) {
		document.querySelectorAll('progress.progress').forEach(function (bar) {
			var target = bar.value;
			bar.value = 0;
			var progObserver = new IntersectionObserver(function (entries) {
				entries.forEach(function (entry) {
					if (entry.isIntersecting) {
						var startTime2 = null;
						function animBar(ts) {
							if (!startTime2) startTime2 = ts;
							var p = Math.min((ts - startTime2) / 1100, 1);
							bar.value = p * target;
							if (p < 1) requestAnimationFrame(animBar);
							else bar.value = target;
						}
						requestAnimationFrame(animBar);
						progObserver.unobserve(entry.target);
					}
				});
			}, { threshold: 0.5 });
			progObserver.observe(bar);
		});
	}

	/* ==========================================================
	   11. KEYBOARD SHORTCUTS
	========================================================== */
	document.addEventListener('keydown', function (e) {
		// Ctrl/Cmd + B: toggle sidebar on mobile
		if (e.key === 'b' && (e.ctrlKey || e.metaKey)) {
			e.preventDefault();
			if (sidebar && sidebar.classList.contains('open')) closeSidebar();
			else openSidebar();
		}
		// Escape: close sidebar on mobile
		if (e.key === 'Escape') {
			closeSidebar();
		}
	});

	/* ==========================================================
	   12. FORM VALIDATION FEEDBACK TOAST
	========================================================== */
	document.querySelectorAll('form[data-toast-success]').forEach(function (form) {
		form.addEventListener('submit', function () {
			var msg = form.getAttribute('data-toast-success');
			if (msg) window.SRToast.show(msg, 'success');
		});
	});

	/* ==========================================================
	   13. FIELD HELP POPOVERS
	========================================================== */
	function hideFieldHelpPopovers(except) {
		document.querySelectorAll('.field-help-trigger').forEach(function (trigger) {
			if (except && trigger === except) return;
			trigger.dataset.helpPinned = '';
			var instance = bootstrap.Popover.getInstance(trigger);
			if (instance) instance.hide();
		});
	}

	if (window.bootstrap && bootstrap.Popover) {
		document.querySelectorAll('.field-help-trigger').forEach(function (trigger) {
			if (trigger.dataset.helpBound === '1') return;
			trigger.dataset.helpBound = '1';

			var instance = bootstrap.Popover.getOrCreateInstance(trigger, {
				trigger: 'manual',
				html: false,
				sanitize: true
			});

			var hideTimer = null;

			function showPopover(pin) {
				clearTimeout(hideTimer);
				hideFieldHelpPopovers(trigger);
				trigger.dataset.helpPinned = pin ? '1' : '';
				instance.show();
			}

			function hidePopoverDelayed() {
				if (trigger.dataset.helpPinned === '1') return;
				clearTimeout(hideTimer);
				hideTimer = setTimeout(function () { instance.hide(); }, 120);
			}

			trigger.addEventListener('mouseenter', function () { showPopover(false); });
			trigger.addEventListener('focus', function () { showPopover(false); });
			trigger.addEventListener('mouseleave', hidePopoverDelayed);
			trigger.addEventListener('blur', hidePopoverDelayed);
			trigger.addEventListener('click', function (e) {
				e.preventDefault();
				clearTimeout(hideTimer);
				if (trigger.dataset.helpPinned === '1') {
					trigger.dataset.helpPinned = '';
					instance.hide();
					return;
				}

				showPopover(true);
			});
		});

		document.addEventListener('click', function (e) {
			if (e.target.closest('.field-help-trigger') || e.target.closest('.popover')) return;
			hideFieldHelpPopovers();
		});
	}

	/* ==========================================================
	   14. CSP COMPLIANCE HELPERS
	   Replaces inline oninput, onclick, and custom view scripts.
	========================================================== */
	// 14a. Number input length limiting
	document.addEventListener('input', function (e) {
		var el = e.target;
		if (el.tagName === 'INPUT' && (el.type === 'number' || el.getAttribute('type') === 'number')) {
			var step = el.getAttribute('step');
			var maxLength = (step === '0.01') ? 5 : 3;
			if (el.value.length > maxLength) {
				el.value = el.value.slice(0, maxLength);
			}
		}
	}, true);

	// 14b. Confirmation dialogs (use data-confirm="Message")
	document.addEventListener('click', function (e) {
		var el = e.target.closest('[data-confirm]');
		if (el) {
			var message = el.getAttribute('data-confirm');
			if (!confirm(message)) {
				e.preventDefault();
				e.stopImmediatePropagation();
			}
		}
	}, true);

	// 14c. Dynamic Measure Selection (replaces inline scripts in Commissioni/Details)
	document.addEventListener('change', function (e) {
		var ids = ['newMisuraTypeSelect', 'addMisuraTypeSelect', 'addMisuraTypeSelectFull'];
		if (ids.indexOf(e.target.id) !== -1) {
			var btnId = e.target.id.replace('Select', '').replace('new', 'btnCreaCollega').replace('add', 'btnAggiungi');
			var btn = document.getElementById(btnId);
			if (btn) {
				var url = new URL(btn.href, window.location.origin);
				url.searchParams.set('typeId', e.target.value);
				btn.href = url.pathname + url.search;
			}
		}
	});

	/* ==========================================================
	   15. MOBILE NAVIGATION HANDLERS
	========================================================== */
	var mobileQuickActionBtn = document.getElementById('mobileQuickActionBtn');
	if (mobileQuickActionBtn) {
		mobileQuickActionBtn.addEventListener('click', function() {
			// Azione rapida: reindirizza alla creazione cliente o mostra un toast
			window.SRToast.show('Funzione rapida: Apertura Registro Clienti...', 'info');
			setTimeout(function() {
				window.location.href = '/Clienti/Index';
			}, 500);
		});
	}

}());
