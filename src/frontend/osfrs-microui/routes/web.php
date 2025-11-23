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

Route::redirect('/', '/microui/auth/login');