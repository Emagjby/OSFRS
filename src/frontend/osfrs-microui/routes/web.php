<?php

use Illuminate\Support\Facades\Route;

Route::view('/microui/auth/login', 'microui.auth.login');
Route::view('/microui/auth/register', 'microui.auth.register');

Route::view('/microui/facility/list', 'microui.facility.list');
Route::view('/microui/facility/create', 'microui.facility.create');
Route::view('/microui/facility/get', 'microui.facility.get');
Route::view('/microui/facility/update', 'microui.facility.update');
Route::view('/microui/facility/delete', 'microui.facility.delete');
Route::view('/microui/facility/availability', 'microui.facility.availability');
Route::view('/microui/facility/availability-update', 'microui.facility.availability-update');

Route::view('/microui/maintenance/list-by-facility', 'microui.maintenance.list-by-facility');
Route::view('/microui/maintenance/upcoming', 'microui.maintenance.upcoming');
Route::view('/microui/maintenance/schedule', 'microui.maintenance.schedule');
Route::view('/microui/maintenance/update', 'microui.maintenance.update');
Route::view('/microui/maintenance/delete', 'microui.maintenance.delete');
Route::view('/microui/maintenance/sync-statuses', 'microui.maintenance.sync-statuses');

Route::view('/microui/reservations/list', 'microui.reservations.list');
Route::view('/microui/reservations/calendar', 'microui.reservations.calendar');
Route::view('/microui/reservations/search', 'microui.reservations.search');
Route::view('/microui/reservations/get', 'microui.reservations.get');
Route::view('/microui/reservations/create', 'microui.reservations.create');
Route::view('/microui/reservations/update', 'microui.reservations.update');
Route::view('/microui/reservations/delete', 'microui.reservations.delete');
Route::view('/microui/reservations/cancel', 'microui.reservations.cancel');
Route::view('/microui/reservations/my', 'microui.reservations.my');
Route::view('/microui/reservations/update-admin', 'microui.reservations.update-admin');

Route::view('/microui/profile/view', 'microui.profile.view');
Route::view('/microui/profile/update', 'microui.profile.update');

Route::redirect('/', '/microui/auth/login');
